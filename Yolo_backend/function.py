import cv2
import numpy as np

#画图
def draw_rounded_rect(img, rect_start, rect_end, corner_width, box_color):

    x1, y1 = rect_start
    x2, y2 = rect_end
    w = corner_width

    # draw filled rectangles
    cv2.rectangle(img, (x1 + w, y1), (x2 - w, y1 + w), box_color, -1)
    cv2.rectangle(img, (x1 + w, y2 - w), (x2 - w, y2), box_color, -1)
    cv2.rectangle(img, (x1, y1 + w), (x1 + w, y2 - w), box_color, -1)
    cv2.rectangle(img, (x2 - w, y1 + w), (x2, y2 - w), box_color, -1)
    cv2.rectangle(img, (x1 + w, y1 + w), (x2 - w, y2 - w), box_color, -1)


    # draw filled ellipses
    cv2.ellipse(img, (x1 + w, y1 + w), (w, w),
                angle = 0, startAngle = -90, endAngle = -180, color = box_color, thickness = -1)

    cv2.ellipse(img, (x2 - w, y1 + w), (w, w),
                angle = 0, startAngle = 0, endAngle = -90, color = box_color, thickness = -1)

    cv2.ellipse(img, (x1 + w, y2 - w), (w, w),
                angle = 0, startAngle = 90, endAngle = 180, color = box_color, thickness = -1)

    cv2.ellipse(img, (x2 - w, y2 - w), (w, w),
                angle = 0, startAngle = 0, endAngle = 90, color = box_color, thickness = -1)

    return img

def draw_text(
        img,
        msg,
        width=8,
        font=cv2.FONT_HERSHEY_SIMPLEX,
        pos=(0, 0),
        font_scale=1,
        font_thickness=2,
        text_color=(0, 255, 0),
        text_color_bg=(0, 0, 0),
        box_offset=(20, 10),
):
    offset = box_offset
    x, y = pos
    text_size, _ = cv2.getTextSize(msg, font, font_scale, font_thickness)
    text_w, text_h = text_size
    rec_start = tuple(p - o for p, o in zip(pos, offset))
    rec_end = tuple(m + n - o for m, n, o in zip((x + text_w, y + text_h), offset, (25, 0)))

    img = draw_rounded_rect(img, rec_start, rec_end, width, text_color_bg)

    cv2.putText(
        img,
        msg,
        (int(rec_start[0] + 6), int(y + text_h + font_scale - 1)),
        font,
        font_scale,
        text_color,
        font_thickness,
        cv2.LINE_AA,
    )

    return text_size



def _show_feedback(frame, c_frame, dict_maps):
    """
            在图像上显示反馈信息。

            参数:
                frame: 当前图像帧。
                c_frame: 反馈状态的布尔数组。
                dict_maps: 反馈信息映射。
                lower_hips_disp: 是否显示"降低臀部"的提示。

            返回:
                frame: 带有反馈信息的图像帧。
            """
    for idx in np.where(c_frame)[0]:
        draw_text(frame, dict_maps[idx][0], pos=(30, dict_maps[idx][1]), text_color=(255, 255, 230),
                  font_scale=0.6,
                  text_color_bg=dict_maps[idx][2])
    return frame  # 返回添加了反馈信息的图像帧

def calculate_angle(point1, point2, point3=None, reference_direction='horizontal'):
    """
    计算角度的函数。
    
    如果提供三个点，则计算由这三个点形成的夹角（point1-point2-point3）。
    如果只提供两个点，则计算由这两个点形成的直线与指定参考方向的夹角（0-90度）。
    
    参数:
        point1: 第一个关键点的坐标 (x1, y1)。
        point2: 第二个关键点的坐标 (x2, y2)。
        point3: (可选) 第三个关键点的坐标 (x3, y3)。
        reference_direction (str): 参考方向，可以是 'horizontal'（水平）或 'vertical'（竖直）。
        
    返回:
        float: 计算出的角度值（单位为度）。
    """
    point1 = np.asarray(point1)  # 转换为numpy数组
    point2 = np.asarray(point2)  # 转换为numpy数组

    # 如果提供了第三个点，计算三点夹角
    if point3 is not None:
        point3 = np.asarray(point3)  # 转换为numpy数组
        
        # 创建向量
        vector1 = point1 - point2  # 从point2指向point1的向量
        vector2 = point3 - point2  # 从point2指向point3的向量
        
        # 计算向量间的夹角
        cosine_angle = np.dot(vector1, vector2) / (np.linalg.norm(vector1) * np.linalg.norm(vector2))
        # 限制cosine_angle值范围，避免因浮点数精度出现NaN
        cosine_angle = np.clip(cosine_angle, -1.0, 1.0)
        # 计算夹角（弧度）
        angle = np.arccos(cosine_angle)
        # 将弧度转换为角度
        angle_degrees = np.degrees(angle)
        
        return angle_degrees
    
    # 如果只提供了两个点，计算与参考方向的夹角
    else:
        # 计算坐标差值
        delta_y = point2[1] - point1[1]
        delta_x = point2[0] - point1[0]

        # 计算连线与水平面的夹角
        angle = np.arctan2(abs(delta_y), abs(delta_x))  # 弧度
        angle_degrees = np.degrees(angle)  # 转换为度

        if reference_direction == 'horizontal':
            angle = angle_degrees
        elif reference_direction == 'vertical':
            angle = 90 - angle_degrees  # 返回与竖直方向的夹角

        return angle

def find_angle(p1, p2, ref_pt=np.array([0, 1])):
    # 计算p1和p2之间的向量
    vector_p1p2 = p2[:2] - p1[:2]
    # 计算向量p1p2和参考方向的夹角
    cos_theta = np.dot(vector_p1p2, ref_pt) / (np.linalg.norm(vector_p1p2) * np.linalg.norm(ref_pt))
    # 限制cos_theta值范围，避免因浮动出现NaN
    cos_theta = np.clip(cos_theta, -1.0, 1.0)
    # 计算夹角（弧度）
    theta = np.arccos(cos_theta)
    # 将弧度转换为角度
    degree = np.degrees(theta)
    return int(degree)


# 画竖直虚线
def draw_dotted_line(frame, lm_coord, start, end, line_color): #画线
    pix_step = 0

    for i in range(start, end+1, 8):
        cv2.circle(frame, (lm_coord[0], i+pix_step), 2, line_color, -1, lineType=cv2.LINE_AA)

    return frame