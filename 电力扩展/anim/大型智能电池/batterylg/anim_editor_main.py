# anim_editor_main.py
# v8.2 - 字体统一 (微软雅黑) / 镜像翻转功能 / 界面美化

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import xml.etree.ElementTree as ET
import os
import sys
import traceback
import math 

# === 依赖检测 ===
HAS_PIL = False
try:
    from PIL import Image, ImageTk
    HAS_PIL = True
except ImportError:
    pass

HAS_DND = False
try:
    from tkinterdnd2 import DND_FILES, TkinterDnD
    HAS_DND = True
except ImportError:
    pass

try:
    from kanim_renderer import KanimRenderer, HAS_PIL as RENDERER_HAS_PIL
    if not RENDERER_HAS_PIL: HAS_PIL = False
except ImportError:
    HAS_PIL = False

class ScmlEditorApp:
    def __init__(self, root):
        self.root = root
        self.root.title("缺氧 SCML 编辑器 v8.2 (美化版)")
        self.root.geometry("1600x950")

        # === 全局字体配置 ===
        # 使用微软雅黑 UI，在 Windows 上显示效果最佳
        self.font_main = ("Microsoft YaHei UI", 9)
        self.font_bold = ("Microsoft YaHei UI", 9, "bold")
        self.font_small = ("Microsoft YaHei UI", 8)
        
        # 配置 ttk 样式
        self.style = ttk.Style()
        self.style.configure("Treeview", font=self.font_main, rowheight=25)
        self.style.configure("Treeview.Heading", font=self.font_bold)
        self.style.configure("TCombobox", font=self.font_main)

        # === 数据 ===
        self.file_path = ""
        self.original_tree = None
        self.current_tree = None
        self.root_xml = None
        self.file_map = {} 
        self.anim_map = {}
        self.images = {}
        self.thumbnails = {}
        
        self.current_anim_id = None
        self.current_time = 0
        self.is_playing = False
        self.play_job = None
        self.loop_play = tk.BooleanVar(value=True)
        
        # 选项
        self.show_grid = tk.BooleanVar(value=True)
        self.show_debug = tk.BooleanVar(value=True)
        self.global_flip = tk.BooleanVar(value=False)
        self.lock_scale = tk.BooleanVar(value=True)
        
        # 批量模式开关
        self.batch_mode = tk.BooleanVar(value=False)
        self.rot_mode = tk.IntVar(value=0) 
        
        self.anim_check_vars = {} 

        self.setup_ui()
        
        if HAS_PIL:
            self.renderer = KanimRenderer(self.canvas)
        else:
            self.renderer = None
            messagebox.showwarning("警告", "缺少 Pillow 库，预览不可用")
        
        if HAS_DND:
            self.root.drop_target_register(DND_FILES)
            self.root.dnd_bind('<<Drop>>', self.on_drop)
            
        self.check_startup_file()

    def check_startup_file(self):
        if len(sys.argv) > 1 and sys.argv[1].endswith(".scml"):
            self.root.after(200, lambda: self.load_file_from_path(sys.argv[1]))

    def on_drop(self, event):
        f = event.data
        if f.startswith('{') and f.endswith('}'): f = f[1:-1]
        self.load_file_from_path(f)

    # === UI 构建 ===
    def setup_ui(self):
        # 1. 顶部栏
        top = tk.Frame(self.root, pady=8, padx=10, bg="#f0f0f0")
        top.pack(fill="x")
        
        btn_cfg = {"font": self.font_main, "bg": "#e0e0e0", "relief": "groove"}
        
        tk.Button(top, text="📂 打开文件", command=self.load_file, **btn_cfg).pack(side="left")
        tk.Button(top, text="💾 保存修改", command=self.save_file, font=self.font_main, bg="#4CAF50", fg="white", relief="flat").pack(side="left", padx=5)
        
        self.lbl_status = tk.Label(top, text="就绪", bg="#f0f0f0", font=self.font_main, fg="#555")
        self.lbl_status.pack(side="left", padx=10)
        self.lbl_mouse = tk.Label(top, text="Mouse: (0, 0)", width=20, bg="#f0f0f0", font=self.font_small, fg="#888")
        self.lbl_mouse.pack(side="right")

        # 2. 分栏
        self.pane = tk.PanedWindow(self.root, orient="horizontal", sashwidth=4, bg="#dcdcdc")
        self.pane.pack(fill="both", expand=True, padx=5, pady=5)

        # --- 左侧：预览区 ---
        left_frame = tk.Frame(self.pane, bg="#333")
        self.pane.add(left_frame, minsize=400)

        # 预览控制栏
        preview_bar = tk.Frame(left_frame, bg="#444", pady=5)
        preview_bar.pack(fill="x")
        tk.Label(preview_bar, text="当前预览:", fg="white", bg="#444", font=self.font_bold).pack(side="left", padx=5)
        self.combo_anim = ttk.Combobox(preview_bar, state="readonly", width=25, font=self.font_main)
        self.combo_anim.pack(side="left", padx=5)
        self.combo_anim.bind("<<ComboboxSelected>>", self.on_anim_combo_change)

        # 画布
        self.canvas = tk.Canvas(left_frame, bg="#2b2b2b", highlightthickness=0)
        self.canvas.pack(fill="both", expand=True)
        
        self.canvas.bind("<ButtonPress-1>", self.on_canvas_click)
        self.canvas.bind("<B1-Motion>", self.on_canvas_drag)
        self.canvas.bind("<MouseWheel>", self.on_canvas_zoom)
        self.canvas.bind("<Button-4>", self.on_canvas_zoom)
        self.canvas.bind("<Button-5>", self.on_canvas_zoom)
        self.canvas.bind("<Button-3>", self.on_canvas_rclick)

        # 播放条
        play_bar = tk.Frame(left_frame, bg="#3c3f41", pady=5)
        play_bar.pack(fill="x")
        tk.Button(play_bar, text="⏮", command=lambda: self.set_time(0), width=3, bg="#555", fg="white", font=self.font_small, relief="flat").pack(side="left", padx=2)
        self.btn_play = tk.Button(play_bar, text="▶", command=self.toggle_play, width=3, bg="#4CAF50", fg="white", font=self.font_bold, relief="flat")
        self.btn_play.pack(side="left", padx=2)
        tk.Checkbutton(play_bar, text="循环", variable=self.loop_play, bg="#3c3f41", fg="#ccc", selectcolor="#555", font=self.font_small).pack(side="left", padx=5)
        
        self.scale_time = tk.Scale(play_bar, from_=0, to=100, orient="horizontal", command=self.on_time_slide, bg="#3c3f41", fg="#eee", showvalue=0, bd=0, troughcolor="#555")
        self.scale_time.pack(side="left", fill="x", expand=True, padx=10)
        self.lbl_frame = tk.Label(play_bar, text="0ms", bg="#3c3f41", fg="white", width=6, font=self.font_small)
        self.lbl_frame.pack(side="right")
        
        # 视图选项
        opt_bar = tk.Frame(left_frame, bg="#eee")
        opt_bar.pack(fill="x")
        chk_cfg = {"bg": "#eee", "font": self.font_small}
        tk.Checkbutton(opt_bar, text="网格", variable=self.show_grid, command=self.update_view, **chk_cfg).pack(side="left")
        tk.Checkbutton(opt_bar, text="锚点", variable=self.show_debug, command=self.update_view, **chk_cfg).pack(side="left")
        tk.Checkbutton(opt_bar, text="视图镜像", variable=self.global_flip, command=self.update_view, **chk_cfg).pack(side="left")
        tk.Button(opt_bar, text="重置视角", command=self.reset_camera, font=self.font_small, bg="#fff", relief="solid", bd=1).pack(side="right", padx=5, pady=2)

        # --- 右侧：操作区 ---
        right_frame = tk.Frame(self.pane)
        self.pane.add(right_frame, minsize=550)

        # 1. 部件列表
        tree_group = tk.Frame(right_frame)
        tree_group.pack(fill="both", expand=True, padx=5, pady=5)
        
        t_tools = tk.Frame(tree_group)
        t_tools.pack(fill="x", pady=2)
        tk.Label(t_tools, text="部件列表", font=self.font_bold).pack(side="left")
        tk.Button(t_tools, text="全选部件", command=self.select_all_parts, font=self.font_small, bg="#e3f2fd", relief="flat").pack(side="right")

        cols = ("name", "ox", "cx", "oy", "cy", "os", "cs", "rot")
        self.tree = ttk.Treeview(tree_group, columns=cols, displaycolumns=cols, selectmode="extended")
        headers = ["部件名", "原X", "现X", "原Y", "现Y", "原S", "现S", "角度"]
        widths = [130, 45, 45, 45, 45, 40, 40, 45]
        
        self.tree.heading("#0", text="图"); self.tree.column("#0", width=30, anchor="center")
        for c, h, w in zip(cols, headers, widths):
            self.tree.heading(c, text=h); self.tree.column(c, width=w, anchor="center")
            
        ysb = tk.Scrollbar(tree_group, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscroll=ysb.set)
        self.tree.pack(side="left", fill="both", expand=True)
        ysb.pack(side="right", fill="y")
        self.tree.bind("<<TreeviewSelect>>", self.on_tree_select)

        # 2. 批量操作面板
        op_pane = tk.PanedWindow(right_frame, orient="horizontal", sashwidth=4, bg="#ddd")
        op_pane.pack(fill="x", padx=5, pady=5, expand=False)

        # === 左下：数值调整 ===
        param_frame = tk.LabelFrame(op_pane, text="1. 参数调整", padx=5, pady=5, font=self.font_bold)
        op_pane.add(param_frame, minsize=320)

        # 通用 Entry 样式
        entry_opts = {"font": self.font_main}
        
        # 位置
        r1 = tk.Frame(param_frame); r1.pack(fill="x", pady=2)
        tk.Label(r1, text="位置 X:", font=self.font_main).pack(side="left")
        self.edx = tk.Entry(r1, width=6, **entry_opts); self.edx.insert(0,"0"); self.edx.pack(side="left", padx=2)
        tk.Label(r1, text="Y:", font=self.font_main).pack(side="left")
        self.edy = tk.Entry(r1, width=6, **entry_opts); self.edy.insert(0,"0"); self.edy.pack(side="left", padx=2)
        tk.Button(r1, text="应用位置", command=self.apply_offset, bg="#e1f5fe", font=self.font_small, relief="flat").pack(side="right")

        # 缩放
        r2 = tk.Frame(param_frame); r2.pack(fill="x", pady=2)
        tk.Label(r2, text="缩放 X:", font=self.font_main).pack(side="left")
        self.val_sx = tk.StringVar(value="1.0"); self.esc_x = tk.Entry(r2, width=6, textvariable=self.val_sx, **entry_opts); self.esc_x.pack(side="left", padx=2)
        self.esc_x.bind("<KeyRelease>", self.on_scale_input)
        tk.Label(r2, text="Y:", font=self.font_main).pack(side="left")
        self.val_sy = tk.StringVar(value="1.0"); self.esc_y = tk.Entry(r2, width=6, textvariable=self.val_sy, **entry_opts); self.esc_y.pack(side="left", padx=2)
        tk.Button(r2, text="应用缩放", command=self.apply_scale, bg="#e1f5fe", font=self.font_small, relief="flat").pack(side="right")
        tk.Checkbutton(r2, text="锁", variable=self.lock_scale, command=self.sync_scale_ui, font=self.font_small).pack(side="right", padx=5)

        # 旋转
        r3 = tk.Frame(param_frame); r3.pack(fill="x", pady=4)
        tk.Label(r3, text="角度 :", font=self.font_main).pack(side="left")
        self.ed_rot = tk.Entry(r3, width=6, **entry_opts); self.ed_rot.insert(0,"0"); self.ed_rot.pack(side="left", padx=2)
        tk.Button(r3, text="应用旋转", command=self.apply_rotation, bg="#e1f5fe", font=self.font_small, relief="flat").pack(side="right")
        
        r_mode_f = tk.Frame(r3); r_mode_f.pack(side="right", padx=5)
        tk.Radiobutton(r_mode_f, text="自旋", variable=self.rot_mode, value=0, font=self.font_small).pack(side="left")
        tk.Radiobutton(r_mode_f, text="绕原点", variable=self.rot_mode, value=1, font=self.font_small).pack(side="left")

        # 新增：镜像翻转 (Flip)
        r4 = tk.Frame(param_frame); r4.pack(fill="x", pady=4)
        tk.Label(r4, text="镜像 :", font=self.font_main).pack(side="left")
        tk.Button(r4, text="水平翻转 (X)", command=lambda: self.apply_flip('x'), bg="#fff9c4", font=self.font_small, relief="flat").pack(side="left", padx=5)
        tk.Button(r4, text="垂直翻转 (Y)", command=lambda: self.apply_flip('y'), bg="#fff9c4", font=self.font_small, relief="flat").pack(side="left", padx=5)

        # 锚点编辑
        tk.Label(param_frame, text="------------------------", fg="#ccc").pack(fill="x")
        p_row = tk.Frame(param_frame); p_row.pack(fill="x")
        tk.Label(p_row, text="全局锚点 X:", fg="gray", font=self.font_small).pack(side="left")
        self.epx = tk.Entry(p_row, width=6, **entry_opts); self.epx.pack(side="left", padx=2)
        tk.Label(p_row, text="Y:", fg="gray", font=self.font_small).pack(side="left")
        self.epy = tk.Entry(p_row, width=6, **entry_opts); self.epy.pack(side="left", padx=2)
        self.btn_apply_pivot = tk.Button(p_row, text="更新", command=self.apply_pivot, state="disabled", font=self.font_small)
        self.btn_apply_pivot.pack(side="right")

        # === 右下：动画列表 ===
        anim_frame = tk.LabelFrame(op_pane, text="2. 批量范围", padx=5, pady=5, font=self.font_bold)
        op_pane.add(anim_frame, minsize=200)

        self.chk_batch = tk.Checkbutton(anim_frame, text="启用批量调整模式", variable=self.batch_mode, command=self.toggle_batch_ui, fg="red", font=self.font_bold)
        self.chk_batch.pack(anchor="w")
        
        tk.Label(anim_frame, text="若勾选，则修改应用到以下列表:", font=self.font_small, fg="gray").pack(anchor="w")

        self.anim_canvas = tk.Canvas(anim_frame, height=120, bg="white")
        sb_anim = tk.Scrollbar(anim_frame, orient="vertical", command=self.anim_canvas.yview)
        self.anim_inner_frame = tk.Frame(self.anim_canvas, bg="white")
        self.anim_inner_frame.bind("<Configure>", lambda e: self.anim_canvas.configure(scrollregion=self.anim_canvas.bbox("all")))
        self.anim_canvas.create_window((0, 0), window=self.anim_inner_frame, anchor="nw")
        self.anim_canvas.configure(yscrollcommand=sb_anim.set)
        
        self.anim_canvas.pack(side="left", fill="both", expand=True)
        sb_anim.pack(side="right", fill="y")
        
        # 底部：还原按钮
        tk.Button(right_frame, text="↺ 还原选中部件 (Reset)", command=self.reset_parts, bg="#ffccbc", font=self.font_bold, relief="flat").pack(fill="x", padx=5, pady=5)

    # === 逻辑实现 ===

    def toggle_batch_ui(self):
        state = "normal" if self.batch_mode.get() else "disabled"
        for widget in self.anim_inner_frame.winfo_children():
            widget.config(state=state)

    def select_all_parts(self):
        self.tree.selection_set(self.tree.get_children())

    def on_anim_combo_change(self, event):
        name = self.combo_anim.get()
        if not name: return
        self.current_anim_id = self.anim_map[name]['id']
        self.scale_time.config(to=self.anim_map[name]['len'])
        self.refresh_list()
        self.update_view()

    def apply_flip(self, axis):
        self._batch_process(flip_axis=axis)

    def apply_rotation(self):
        try:
            dr = float(self.ed_rot.get())
            self._batch_process(d_rot=dr)
        except: pass

    def apply_offset(self):
        try:
            dx = float(self.edx.get()); dy = float(self.edy.get())
            self._batch_process(dx=dx, dy=dy)
        except: pass

    def apply_scale(self):
        try:
            sx = float(self.val_sx.get()); sy = float(self.val_sy.get())
            self._batch_process(sx=sx, sy=sy)
        except: pass

    def _batch_process(self, dx=0, dy=0, sx=None, sy=None, d_rot=None, flip_axis=None):
        sel_items = self.tree.selection()
        target_fkeys = set()
        process_all_parts = False 
        
        if not sel_items:
            if self.batch_mode.get():
                process_all_parts = True
                if not messagebox.askyesno("全量修改确认", "当前未选中任何部件。\n\n这将修改 [勾选动画] 中的 [所有部件]！\n\n是否继续？"):
                    return
            else:
                messagebox.showwarning("提示", "请先选中要修改的部件！")
                return
        else:
            target_fkeys = {self.tree.item(i, "tags")[0] for i in sel_items}

        target_anims = []
        if self.batch_mode.get():
            for name, var in self.anim_check_vars.items():
                if var.get(): target_anims.append(name)
            if not target_anims:
                messagebox.showwarning("提示", "批量模式下请至少勾选一个动画！")
                return
        else:
            curr_name = self.combo_anim.get()
            if curr_name: target_anims.append(curr_name)

        count = 0
        rot_mode = self.rot_mode.get() 

        for anim_name in target_anims:
            aid = self.anim_map[anim_name]['id']
            anode = self.root_xml.find(f".//entity/animation[@id='{aid}']")
            
            for t in anode.findall("timeline"):
                k0 = t.find("key/object")
                if k0 is None: continue
                fk = f"{k0.attrib.get('folder','0')}:{k0.attrib.get('file','0')}"
                
                if process_all_parts or (fk in target_fkeys):
                    for k in t.findall("key"):
                        for o in k.findall("object"):
                            
                            cur_x = float(o.attrib.get('x', 0))
                            cur_y = float(o.attrib.get('y', 0))
                            cur_a = float(o.attrib.get('angle', 0))
                            
                            # A. 镜像翻转 (Flip)
                            if flip_axis == 'x':
                                csx = float(o.attrib.get('scale_x', 1))
                                o.attrib['scale_x'] = f"{-csx:.4f}"
                            elif flip_axis == 'y':
                                csy = float(o.attrib.get('scale_y', 1))
                                o.attrib['scale_y'] = f"{-csy:.4f}"

                            # B. 缩放
                            if sx is not None:
                                csx = float(o.attrib.get('scale_x', 1))
                                csy = float(o.attrib.get('scale_y', 1))
                                o.attrib['scale_x'] = f"{csx * sx:.4f}"
                                o.attrib['scale_y'] = f"{csy * sy:.4f}"
                                cur_x *= sx
                                cur_y *= sy
                                o.attrib['x'] = f"{cur_x:.4f}"
                                o.attrib['y'] = f"{cur_y:.4f}"

                            # C. 位置
                            if dx != 0 or dy != 0:
                                cur_x += dx
                                cur_y += dy
                                o.attrib['x'] = f"{cur_x:.4f}"
                                o.attrib['y'] = f"{cur_y:.4f}"

                            # D. 旋转
                            if d_rot is not None and d_rot != 0:
                                if rot_mode == 0: 
                                    o.attrib['angle'] = f"{(cur_a + d_rot) % 360:.4f}"
                                else:
                                    rad = math.radians(d_rot)
                                    c = math.cos(rad); s = math.sin(rad)
                                    new_x = cur_x * c - cur_y * s
                                    new_y = cur_x * s + cur_y * c
                                    new_a = (cur_a + d_rot) % 360
                                    o.attrib['x'] = f"{new_x:.4f}"
                                    o.attrib['y'] = f"{new_y:.4f}"
                                    o.attrib['angle'] = f"{new_a:.4f}"

                    count += 1
        
        self.update_view()
        self.refresh_list()
        
        mode_str = "批量模式" if self.batch_mode.get() else "当前动画"
        action_str = "修改" if flip_axis is None else "翻转"
        messagebox.showinfo("完成", f"[{mode_str}] {action_str}了 {len(target_anims)} 个动画中的 {count} 个时间轴")

    # === 加载与解析 ===
    def load_file_from_path(self, path):
        try:
            self.file_path = path
            self.original_tree = ET.parse(path)
            self.current_tree = ET.parse(path)
            self.root_xml = self.current_tree.getroot()
            self.parse_res()
            
            for w in self.anim_inner_frame.winfo_children(): w.destroy()
            self.anim_check_vars = {}
            self.anim_map = {}
            anims = []
            
            for a in self.root_xml.findall(".//entity/animation"):
                name = a.attrib['name']
                l = a.attrib.get('length', '100')
                self.anim_map[name] = {'id': a.attrib['id'], 'len': int(l) if l.isdigit() else 100}
                anims.append(name)
                
                var = tk.BooleanVar(value=True) 
                self.anim_check_vars[name] = var
                
                cb = tk.Checkbutton(self.anim_inner_frame, text=name, variable=var, anchor="w", state="disabled", bg="white", font=self.font_main)
                cb.pack(fill="x")

            self.combo_anim['values'] = anims
            if anims: 
                self.combo_anim.current(0)
                self.on_anim_combo_change(None)
                
            self.lbl_status.config(text=f"已加载: {os.path.basename(path)}")
            self.toggle_batch_ui()
            
        except Exception as e:
            traceback.print_exc()
            messagebox.showerror("加载失败", str(e))

    def parse_res(self):
        self.file_map = {}; self.images = {}; self.thumbnails = {}
        base = os.path.dirname(self.file_path)
        for fo in self.root_xml.findall("folder"):
            fid = fo.attrib['id']
            for f in fo.findall("file"):
                k = f"{fid}:{f.attrib['id']}"
                fname = f.attrib['name'].strip()
                self.file_map[k] = {'px':float(f.attrib.get('pivot_x',0)), 'py':float(f.attrib.get('pivot_y',0)), 'name':fname, 'node':f}
                if HAS_PIL:
                    p = os.path.join(base, fname)
                    if os.path.exists(p):
                        img = Image.open(p)
                        self.images[k] = img
                        thumb = img.copy()
                        thumb.thumbnail((20, 20))
                        self.thumbnails[k] = ImageTk.PhotoImage(thumb)

    def refresh_list(self):
        sel_tags = {self.tree.item(i, "tags")[0] for i in self.tree.selection() if self.tree.item(i, "tags")}
        for i in self.tree.get_children(): self.tree.delete(i)
        
        if not self.current_anim_id: return
        anim = self.root_xml.find(f".//entity/animation[@id='{self.current_anim_id}']")
        orig_anim = self.original_tree.find(f".//entity/animation[@id='{self.current_anim_id}']")
        
        for t in anim.findall("timeline"):
            tk_ = None
            for k in t.findall("key"):
                if int(k.attrib.get('time',0)) <= self.current_time: tk_ = k
                else: break
            
            if tk_:
                obj = tk_.find("object")
                fkey = f"{obj.attrib.get('folder','0')}:{obj.attrib.get('file','0')}"
                name = self.file_map.get(fkey, {}).get('name', '?')
                
                orig_obj = None
                orig_t = orig_anim.find(f"timeline[@id='{t.attrib['id']}']") if orig_anim else None
                if orig_t:
                    for k in orig_t.findall("key"):
                         if int(k.attrib.get('time',0)) <= self.current_time: orig_obj = k.find("object")
                         else: break
                
                vals = self.get_obj_vals(obj)
                origs = self.get_obj_vals(orig_obj) if orig_obj is not None else vals
                
                thumb = self.thumbnails.get(fkey, "")
                item = self.tree.insert("", "end", image=thumb, values=(
                    name, 
                    f"{origs['x']:.2f}", f"{vals['x']:.2f}",
                    f"{origs['y']:.2f}", f"{vals['y']:.2f}",
                    f"{origs['sx']:.2f}", f"{vals['sx']:.2f}",
                    f"{vals['a']:.2f}"
                ), tags=(fkey,))
                
                if fkey in sel_tags: self.tree.selection_add(item)

    def get_obj_vals(self, obj):
        return {
            'x': float(obj.attrib.get('x',0)), 'y': float(obj.attrib.get('y',0)),
            'sx': float(obj.attrib.get('scale_x',1)), 'sy': float(obj.attrib.get('scale_y',1)),
            'a': float(obj.attrib.get('angle',0))
        }

    # === 辅助 ===
    def on_scale_input(self, e):
        if self.lock_scale.get(): self.val_sy.set(self.val_sx.get())
    def sync_scale_ui(self):
        if self.lock_scale.get(): self.val_sy.set(self.val_sx.get())
    
    def on_tree_select(self, e):
        sel = self.tree.selection()
        if not sel: 
            self.btn_apply_pivot.config(state="disabled")
            return
        fkey = self.tree.item(sel[0], "tags")[0]
        for i in sel:
            if self.tree.item(i, "tags")[0] != fkey:
                self.btn_apply_pivot.config(state="disabled"); return
        info = self.file_map.get(fkey)
        if info:
            self.epx.delete(0,"end"); self.epx.insert(0, str(info['px']))
            self.epy.delete(0,"end"); self.epy.insert(0, str(info['py']))
            self.btn_apply_pivot.config(state="normal")

    def apply_pivot(self):
        sel = self.tree.selection()
        if not sel: return
        fkey = self.tree.item(sel[0], "tags")[0]
        try:
            nx = float(self.epx.get()); ny = float(self.epy.get())
            self.file_map[fkey]['px'] = nx
            self.file_map[fkey]['py'] = ny
            self.file_map[fkey]['node'].attrib['pivot_x'] = str(nx)
            self.file_map[fkey]['node'].attrib['pivot_y'] = str(ny)
            self.update_view()
        except: pass

    def reset_parts(self):
        sel = self.tree.selection()
        if not sel: return
        fkeys = {self.tree.item(i,"tags")[0] for i in sel}
        
        if self.batch_mode.get():
            target_anims = [k for k,v in self.anim_check_vars.items() if v.get()]
        else:
            target_anims = [self.combo_anim.get()]
        
        c=0
        for aname in target_anims:
            aid = self.anim_map[aname]['id']
            c_anim = self.root_xml.find(f".//entity/animation[@id='{aid}']")
            o_anim = self.original_tree.find(f".//entity/animation[@id='{aid}']")
            if not o_anim: continue
            
            o_map = {}
            for t in o_anim.findall("timeline"):
                k = t.find("key/object")
                if k is not None:
                    fk = f"{k.attrib.get('folder','0')}:{k.attrib.get('file','0')}"
                    o_map[fk] = t
            
            for t in c_anim.findall("timeline"):
                k = t.find("key/object")
                if k is not None:
                    fk = f"{k.attrib.get('folder','0')}:{k.attrib.get('file','0')}"
                    if fk in fkeys and fk in o_map:
                        ot = o_map[fk]
                        for key in t.findall("key"):
                            kid = key.attrib['id']
                            okey = ot.find(f"key[@id='{kid}']")
                            if okey:
                                obj = key.find("object")
                                oobj = okey.find("object")
                                if obj is not None and oobj is not None:
                                    for attr in ['x','y','scale_x','scale_y','angle','a']:
                                        if attr in oobj.attrib: obj.attrib[attr] = oobj.attrib[attr]
                                    c+=1
        self.update_view(); self.refresh_list()
        messagebox.showinfo("还原", f"还原了 {c} 帧")

    def toggle_play(self):
        self.is_playing = not self.is_playing
        self.btn_play.config(text="⏸" if self.is_playing else "▶")
        if self.is_playing: self.play_loop()
    def play_loop(self):
        if not self.is_playing: return
        n = self.combo_anim.get()
        if not n: return
        l = self.anim_map[n]['len']
        t = self.current_time + 33
        if t > l: 
            if self.loop_play.get(): t = 0 
            else: self.is_playing = False; self.btn_play.config(text="▶"); return
        self.set_time(t); self.root.after(33, self.play_loop)
    def set_time(self, t):
        self.current_time = int(t); self.scale_time.set(t); self.lbl_frame.config(text=f"{t}ms"); self.update_view()
    def on_time_slide(self,v): self.set_time(v); self.refresh_list()
    def load_file(self):
        p = filedialog.askopenfilename(filetypes=[("SCML","*.scml")])
        if p: self.load_file_from_path(p)
    def save_file(self):
        p = filedialog.asksaveasfilename(defaultextension=".scml")
        if p: self.current_tree.write(p); messagebox.showinfo("OK", p)
    
    def update_view(self, e=None):
        if not self.renderer or not self.current_anim_id: return
        self.renderer.clear()
        w, h = self.canvas.winfo_width(), self.canvas.winfo_height()
        if w<10: w=800; h=600
        self.renderer.draw_grid(w, h, self.show_grid.get())
        anim = self.root_xml.find(f".//entity/animation[@id='{self.current_anim_id}']")
        ml = anim.find("mainline")
        mk = None
        for k in ml.findall("key"):
            if int(k.attrib.get('time',0)) <= self.current_time: mk = k
            else: break
        if mk:
            self.renderer.render_frame(mk, anim, self.file_map, self.images, self.current_time, w, h, self.show_debug.get(), self.global_flip.get())
    
    def on_canvas_click(self, e): self.canvas.scan_mark(e.x, e.y); self.lx=e.x; self.ly=e.y
    def on_canvas_drag(self, e):
        if not hasattr(self,'lx'): self.lx=e.x; self.ly=e.y
        self.renderer.pan_x += (e.x-self.lx)/self.renderer.zoom
        self.renderer.pan_y -= (e.y-self.ly)/self.renderer.zoom
        self.lx=e.x; self.ly=e.y; self.update_view()
    def on_canvas_zoom(self, e):
        s = 1.1 if (e.delta>0 or e.num==4) else 0.9
        self.renderer.zoom *= s; self.update_view()
    def reset_camera(self): self.renderer.set_camera(0,0,1.0); self.update_view()
    def on_canvas_rclick(self, e):
        self.canvas.delete("highlight")
        p = self.renderer.hit_test(e.x, e.y)
        if p:
            self.renderer.highlight_part(p)
            tid = p['id']
            for i in self.tree.get_children():
                if tid in self.tree.item(i,'tags'):
                    self.tree.selection_set(i); self.tree.see(i); break

if __name__ == "__main__":
    root = TkinterDnD.Tk() if HAS_DND else tk.Tk()
    try:
        from ctypes import windll
        windll.shcore.SetProcessDpiAwareness(1)
    except: pass
    app = ScmlEditorApp(root)
    root.mainloop()