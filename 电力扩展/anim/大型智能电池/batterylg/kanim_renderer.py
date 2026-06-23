# kanim_renderer.py
# v6.0 - 摄像机、交互检测、单模式渲染

import math
try:
    from PIL import Image, ImageTk, ImageDraw
    HAS_PIL = True
except ImportError:
    HAS_PIL = False

class KanimRenderer:
    def __init__(self, canvas):
        self.canvas = canvas
        self.draw_cache = [] 
        # 摄像机状态
        self.zoom = 1.0
        self.pan_x = 0
        self.pan_y = 0
        # 存储当前帧的部件位置信息，用于点击检测
        self.current_frame_parts = [] 

    def clear(self):
        self.canvas.delete("all")
        self.draw_cache = []
        self.current_frame_parts = []

    def set_camera(self, pan_x, pan_y, zoom):
        self.pan_x = pan_x
        self.pan_y = pan_y
        self.zoom = zoom

    def draw_grid(self, width, height, show_grid):
        if not show_grid: return

        # 计算屏幕边缘对应的世界坐标
        # Screen = (World + Pan) * Zoom + Center
        # World = (Screen - Center) / Zoom - Pan
        cx, cy = width / 2, height / 2
        
        step = 100 # 网格间距
        
        # 找到视野范围内的网格线索引
        start_x = int(((0 - cx) / self.zoom - self.pan_x) / step) - 1
        end_x = int(((width - cx) / self.zoom - self.pan_x) / step) + 1
        
        start_y = int(((0 - cy) / self.zoom + self.pan_y) / step) - 1 # 注意Y轴方向
        end_y = int(((height - cy) / self.zoom + self.pan_y) / step) + 1

        # 绘制网格线
        for i in range(start_x, end_x + 1):
            world_x = i * step
            sx = (world_x + self.pan_x) * self.zoom + cx
            self.canvas.create_line(sx, 0, sx, height, fill="#444", dash=(2, 4), tags="grid")

        for i in range(start_y, end_y + 1):
            world_y = i * step # 这里的world_y是数学坐标(上正下负)
            # 屏幕Y = CenterY - (WorldY + PanY) * Zoom
            # 注意: 之前的逻辑是 cy - y。我们要统一逻辑。
            # 设 world_y 为正，屏幕上应该在中心上方。
            # screen_y = cy - (world_y + self.pan_y) * self.zoom 
            # 等等，之前的逻辑是 screen_y = cy - (y + pan) ? 
            # 让我们保持 render_frame 里的逻辑一致：
            # draw_cy = cy - (y * zoom) + (pan_y * zoom) ❌
            # 正确的摄像机逻辑：ScreenY = CenterY - (WorldY - PanY) * Zoom
            
            # 为了简化，我们只画线。
            sy = cy - (world_y + self.pan_y) * self.zoom
            self.canvas.create_line(0, sy, width, sy, fill="#444", dash=(2, 4), tags="grid")

        # 轴线
        origin_x = (0 + self.pan_x) * self.zoom + cx
        origin_y = cy - (0 + self.pan_y) * self.zoom
        
        self.canvas.create_line(origin_x, 0, origin_x, height, fill="#d32f2f", width=2, tags="grid")
        self.canvas.create_line(0, origin_y, width, origin_y, fill="#d32f2f", width=2, tags="grid")

    def render_frame(self, mk, anim_node, file_map, images, current_time, 
                    width, height, show_debug=True, global_flip=False):
        if not HAS_PIL: return

        cx, cy = width / 2, height / 2

        # 1. 收集并排序
        draw_list = []
        for ref in mk.findall("object_ref"):
            z = int(ref.attrib.get('z_index', 0))
            timeline = anim_node.find(f"timeline[@id='{ref.attrib['timeline']}']")
            if timeline is None: continue

            tk_ = None
            if 'key' in ref.attrib:
                tk_ = timeline.find(f"key[@id='{ref.attrib['key']}']")
            else:
                for k in timeline.findall("key"):
                    if int(k.attrib.get('time', 0)) <= current_time: tk_ = k
                    else: break
            
            if tk_ is None: continue
            obj = tk_.find("object")
            if obj is None: continue
            
            draw_list.append({'z': z, 'obj': obj})

        draw_list.sort(key=lambda x: x['z'])

        # 2. 绘制
        for item in draw_list:
            obj = item['obj']
            fk = f"{obj.attrib.get('folder', '0')}:{obj.attrib.get('file', '0')}"
            
            # 读取属性
            x = float(obj.attrib.get('x', 0))
            y = float(obj.attrib.get('y', 0))
            angle = float(obj.attrib.get('angle', 0))
            sx = float(obj.attrib.get('scale_x', 1))
            sy = float(obj.attrib.get('scale_y', 1))

            if global_flip:
                x = -x
                angle = -angle
                sx = -sx

            self._draw_object(fk, x, y, angle, sx, sy, cx, cy, file_map, images, show_debug)

    def _draw_object(self, fk, x, y, angle, sx, sy, cx, cy, file_map, images, show_debug):
        info = file_map.get(fk)
        img = images.get(fk)
        if not img or not info: return

        # --- 变换计算 (Mode A Only) ---
        px, py = info['px'], info['py']
        
        # 翻转处理
        if sx < 0:
            img = img.transpose(Image.FLIP_LEFT_RIGHT)
            px = 1.0 - px
            sx = abs(sx)
        if sy < 0:
            img = img.transpose(Image.FLIP_TOP_BOTTOM)
            py = 1.0 - py
            sy = abs(sy)

        # 缩放处理 (包含摄像机缩放)
        # 最终显示大小 = 图片原大 * 对象缩放 * 摄像机缩放
        final_scale_x = sx * self.zoom
        final_scale_y = sy * self.zoom
        
        w, h = img.size
        w_view, h_view = int(w * final_scale_x), int(h * final_scale_y)
        if w_view <= 0 or h_view <= 0: return
        
        img_view = img.resize((w_view, h_view), Image.Resampling.NEAREST)

        # 锚点像素位置 (在缩放后的图上)
        p_pixel_x = w_view * px
        p_pixel_y = h_view * (1.0 - py)

        # 旋转
        img_rot = img_view.rotate(angle, expand=True, resample=Image.Resampling.BILINEAR)
        
        # 旋转偏移计算
        c_rot_x, c_rot_y = w_view / 2, h_view / 2
        v_x = p_pixel_x - c_rot_x
        v_y = p_pixel_y - c_rot_y
        
        rad = math.radians(angle)
        v_x_rot = v_x * math.cos(rad) - v_y * math.sin(rad)
        v_y_rot = v_x * math.sin(rad) + v_y * math.cos(rad)

        # 屏幕坐标计算
        # ScreenX = CenterX + (WorldX + PanX) * Zoom
        # ScreenY = CenterY - (WorldY + PanY) * Zoom
        screen_x = cx + (x + self.pan_x) * self.zoom
        screen_y = cy - (y + self.pan_y) * self.zoom
        
        # 最终绘制坐标 (TopLeft)
        draw_cx = screen_x - v_x_rot
        draw_cy = screen_y - v_y_rot
        
        rot_w, rot_h = img_rot.size
        final_x = draw_cx - rot_w / 2
        final_y = draw_cy - rot_h / 2

        # 绘制
        tk_img = ImageTk.PhotoImage(img_rot)
        self.draw_cache.append(tk_img)
        self.canvas.create_image(final_x, final_y, anchor="nw", image=tk_img, tags="part")

        # --- 存储点击检测数据 ---
        # 我们保存图片的包围盒信息，用于后续 hit_test
        self.current_frame_parts.append({
            'name': info['name'],
            'rect': (final_x, final_y, final_x + rot_w, final_y + rot_h),
            'tk_img': tk_img, # 保存引用以便高亮时重绘
            'id': fk,
            'screen_pos': (final_x, final_y) # 简化用于高亮
        })

        if show_debug:
            self.canvas.create_oval(screen_x-3, screen_y-3, screen_x+3, screen_y+3, fill="red", tags="debug")

    def hit_test(self, mouse_x, mouse_y):
        # 简单的逆序遍历检测 (从最上层开始检测)
        for part in reversed(self.current_frame_parts):
            x1, y1, x2, y2 = part['rect']
            # 这里做的是 AABB 包围盒检测，如果图片旋转角度很大，可能点中空白区域也算。
            # 完美做法是逆旋转鼠标坐标，但对于工具来说 AABB 够用了。
            if x1 <= mouse_x <= x2 and y1 <= mouse_y <= y2:
                return part
        return None

    def highlight_part(self, part):
        # 在部件周围画一个框
        x1, y1, x2, y2 = part['rect']
        self.canvas.create_rectangle(x1, y1, x2, y2, outline="#00ff00", width=2, tags="highlight")
        # 显示名字标签
        self.canvas.create_text(x1, y1-10, text=part['name'], fill="#00ff00", anchor="sw", font=("Arial", 10, "bold"), tags="highlight")