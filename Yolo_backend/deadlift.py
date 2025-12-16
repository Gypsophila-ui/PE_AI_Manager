import numpy as np
from function import draw_text, calculate_angle, _show_feedback, draw_dotted_line
import cv2
import time

class DeadliftTracker:
    def __init__(self):
        """
        初始化硬拉状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'DISPLAY_TEXT': np.full((4,), False),
            'COUNT_FRAMES': np.zeros((4,), dtype=np.int64),
            'INCORRECT_POSTURE': False,
            'curr_state': None,
            'DEADLIFT_COUNT': 0,
            'IMPROPER_DEADLIFT': 0
        }

        # 添加事件跟踪
        self.events = []  # 存储关键事件
        self.current_deadlift_id = None
        self.deadlift_start_frame = None
        self.last_improper_count = 0

        # 设置反馈信息映射
        self.FEEDBACK_ID_MAP = {
            0: ('BACK TOO ARCHED', 215, (0, 153, 255)),  # 背部过度弯曲
            1: ('KNEE TOO LOW', 215, (0, 153, 255)),     # 膝盖过低
            2: ('ARM BENDING', 125, (255, 80, 80))       # 手臂弯曲
        }

        # 获取阈值
        self.thresholds = self.get_thresholds_beginner()

    def get_thresholds(self):
        """
        获取阈值配置。

        返回:
            dict: 阈值字典。
        """
        return self.get_thresholds_beginner()

    def get_thresholds_beginner(self):
        """获取初学者模式的阈值设置"""
        thresholds = {
            'SHOULDER_HIP_KNEE': {
                'NORMAL': (45, 90),  # 初始阶段
                'TRANS': (95, 130),  # 过渡阶段
                'PASS': (135, 180)    # 完成阶段
            },
            'HIP_THRESH': 50,      # 髋关节角度阈值
            'KNEE_THRESH': 45,     # 膝关节角度范围
            'ARM_THRESH': 150,
            'INACTIVE_THRESH': 15.0,   # 静止时间阈值 (秒)
            'CNT_FRAME_THRESH': 50     # 维持帧数阈值
        }
        return thresholds

    def _get_state(self, shoulder_hip_knee_angle):
        angle = None
        # 根据肩-髋-膝夹角判断阶段
        if self.thresholds['SHOULDER_HIP_KNEE']['NORMAL'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['NORMAL'][1]:
            angle = 1  # 下放阶段，肩-髋-膝角度在正常范围
        elif self.thresholds['SHOULDER_HIP_KNEE']['TRANS'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['TRANS'][1]:
            angle = 2  # 过渡阶段，肩-髋-膝角度处于过渡范围
        elif self.thresholds['SHOULDER_HIP_KNEE']['PASS'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['PASS'][1]:
            angle = 3  # 完成阶段，肩-髋-膝角度达到最大值

        return f's{angle}' if angle else None  # 返回对应的状态，若无有效状态则返回 None

    def _update_state_sequence(self, state):
        """
        更新状态序列，根据新的状态添加到状态序列中。

        参数:
            state: 当前状态（如 's1', 's2' 等）
        """
        if state == 's2':
            # 添加状态 's2' 到状态序列
            if (('s3' not in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2')) == 0) or \
                    (('s3' in self.state_tracker['state_seq']) and (
                            self.state_tracker['state_seq'].count('s2') == 1)):
                self.state_tracker['state_seq'].append(state)

        elif state == 's3':
            # 添加状态 's3' 到状态序列
            if (state not in self.state_tracker['state_seq']) and 's2' in self.state_tracker['state_seq']:
                self.state_tracker['state_seq'].append(state)

    def _get_current_error_reasons(self):
        """获取当前错误原因，包含更多详细信息"""
        errors = []
        for i, is_displaying in enumerate(self.state_tracker['DISPLAY_TEXT']):
            if is_displaying:
                error_id, error_msg, _ = self.FEEDBACK_ID_MAP.get(i, (f"ERROR_{i}", "", (0,0,0)))
                errors.append({
                    'code': error_id,
                    'message': error_msg,
                    'frame_time': time.time(),  # 添加时间戳
                    'error_type': self._get_error_type_by_id(i)  # 添加错误类型分类
                })
        return errors

    def _get_error_type_by_id(self, error_id):
        """根据错误ID返回错误类型"""
        error_types = {
            0: '腰部过度后弯，形成反弓状，背部没有保持自然挺直状态',
            1: '膝盖相对踝关节位置过低，可能是起始姿势蹲得过深或者在拉起过程中膝盖过度弯曲',
            2: '手臂在拉起过程中发生弯曲，没有保持直臂状态'
        }
        return error_types.get(error_id, '未知错误')

    def _record_events(self, frame_index, fps):
        """记录关键事件，正确绑定错误信息到每个硬拉动作"""
        # 确保events列表中的每个事件都有correct_count和incorrect_count键
        if self.events:
            for event in self.events:
                if 'correct_count' not in event:
                    event['correct_count'] = self.state_tracker['DEADLIFT_COUNT']
                if 'incorrect_count' not in event:
                    event['incorrect_count'] = self.state_tracker['IMPROPER_DEADLIFT']
        
        # 检测硬拉开始
        if self.state_tracker['curr_state'] == 's2' and self.current_deadlift_id is None:
            self.current_deadlift_id = f"deadlift_{len([e for e in self.events if e['type'] == 'DEADLIFT_START']) + 1}"  # 从1开始编号
            self.deadlift_start_frame = frame_index
            self.events.append({
                'event_id': self.current_deadlift_id,
                'type': 'DEADLIFT_START',
                'frame': frame_index,
                'timestamp': frame_index / fps if fps > 0 else 0,
                'correct_count': self.state_tracker['DEADLIFT_COUNT'],
                'incorrect_count': self.state_tracker['IMPROPER_DEADLIFT'],
                'deadlift_number': len([e for e in self.events if e['type'] == 'DEADLIFT_START']),  # 第几个硬拉
                'current_errors': []  # 初始化错误列表
            })

        # 检查并记录当前错误
        current_errors = self._get_current_error_reasons()
        if self.current_deadlift_id and current_errors:
            # 查找当前硬拉的起始事件
            for event in reversed(self.events):
                if event.get('event_id') == self.current_deadlift_id and event['type'] == 'DEADLIFT_START':
                    # 更新错误信息
                    for error in current_errors:
                        if error not in event.get('current_errors', []):
                            if 'current_errors' not in event:
                                event['current_errors'] = []
                            event['current_errors'].append(error)
                    break

        # 检测硬拉完成（正确）
        if self.current_deadlift_id and self.state_tracker['curr_state'] == 's1':
            # 检查是否是正确完成
            is_correct_completion = (
                len(self.state_tracker['state_seq']) == 3 and 
                not self.state_tracker['INCORRECT_POSTURE']
            )
            
            if is_correct_completion:
                # 查找起始事件以获取错误信息
                start_event = None
                for event in reversed(self.events):
                    if event.get('event_id') == self.current_deadlift_id and event['type'] == 'DEADLIFT_START':
                        start_event = event
                        break
                
                self.events.append({
                    'event_id': self.current_deadlift_id,
                    'type': 'DEADLIFT_COMPLETE_CORRECT',
                    'frame': frame_index,
                    'timestamp': frame_index / fps if fps > 0 else 0,
                    'duration_frames': frame_index - self.deadlift_start_frame if self.deadlift_start_frame else 0,
                    'correct_count': self.state_tracker['DEADLIFT_COUNT'],
                    'incorrect_count': self.state_tracker['IMPROPER_DEADLIFT'],
                    'deadlift_number': start_event.get('deadlift_number', 0) if start_event else 0,
                    'errors_during_deadlift': start_event.get('current_errors', []) if start_event else []
                })
                self.current_deadlift_id = None
                self.deadlift_start_frame = None
            
            # 检查是否是错误完成
            elif self.state_tracker['INCORRECT_POSTURE'] or (
                's2' in self.state_tracker['state_seq'] and 
                len(self.state_tracker['state_seq']) == 1
            ):
                # 查找起始事件以获取错误信息
                start_event = None
                for event in reversed(self.events):
                    if event.get('event_id') == self.current_deadlift_id and event['type'] == 'DEADLIFT_START':
                        start_event = event
                        break
                
                self.events.append({
                    'event_id': self.current_deadlift_id,
                    'type': 'DEADLIFT_COMPLETE_INCORRECT',
                    'frame': frame_index,
                    'timestamp': frame_index / fps if fps > 0 else 0,
                    'error_reasons': self._get_current_error_reasons(),
                    'duration_frames': frame_index - self.deadlift_start_frame if self.deadlift_start_frame else 0,
                    'correct_count': self.state_tracker['DEADLIFT_COUNT'],
                    'incorrect_count': self.state_tracker['IMPROPER_DEADLIFT'],
                    'deadlift_number': start_event.get('deadlift_number', 0) if start_event else 0,
                    'errors_during_deadlift': start_event.get('current_errors', []) if start_event else []
                })
                self.last_improper_count = self.state_tracker['IMPROPER_DEADLIFT']
                self.current_deadlift_id = None
                self.deadlift_start_frame = None

    def track(self, k, im0, ind, count, fps=30):
        """
        处理硬拉检测和计数的主逻辑，包含状态跟踪、姿势检查和反馈显示。

        参数:
        - k: 姿势关键点
        - im0: 当前帧图像
        - ind: 当前处理对象的索引（对于单人模式，始终为0）
        - count: 计数器列表（对于单人模式，只包含一个元素）
        - fps: 视频帧率
        """
        frame_height, frame_width, _ = im0.shape
        
        # 计算角度
        hip_knee_angle = calculate_angle(k[12].cpu(), k[14].cpu(), reference_direction='horizontal')  # 髋膝夹角，参考水平
        shoulder_hip_angle = calculate_angle(k[6].cpu(), k[12].cpu(), reference_direction='horizontal')  # 肩膝夹角，参考水平
        shoulder_hip_knee_angle = shoulder_hip_angle + hip_knee_angle
        
        # 定义关节点坐标
        right_knee = (int(k[14][0].cpu().item()), int(k[14][1].cpu().item()))
        right_hip = (int(k[12][0].cpu().item()), int(k[12][1].cpu().item()))
        right_shoulder = (int(k[6][0].cpu().item()), int(k[6][1].cpu().item()))
        right_ankle = (int(k[16][0].cpu().item()), int(k[16][1].cpu().item()))
        right_wrist = (int(k[10][0].cpu().item()), int(k[10][1].cpu().item()))
        right_elbow = (int(k[8][0].cpu().item()), int(k[8][1].cpu().item()))

        # 绘制辅助线和角度指示器，模仿MediaPipe的效果
        # 绘制竖直虚线
        dotted_line_length = 60
        im0 = draw_dotted_line(im0, right_knee, right_knee[1] - dotted_line_length, right_knee[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_hip, right_hip[1] - dotted_line_length, right_hip[1], (255, 0, 0))

        # 计算各个角度
        shoulder_hip_vertical_angle = calculate_angle(k[6].cpu(), k[12].cpu(), reference_direction="vertical")
        knee_ankle_vertical_angle = calculate_angle(k[14].cpu(), k[16].cpu(), reference_direction="vertical")
        wrist_elbow_angle = calculate_angle(k[8].cpu(), k[10].cpu(), reference_direction="horizontal")
        elbow_shoulder_angle = calculate_angle(k[6].cpu(), k[8].cpu(), reference_direction="horizontal")
        arm_angle = wrist_elbow_angle + elbow_shoulder_angle

        # 绘制骨架连线
        color_light_blue = (204, 204, 0)  # BGR格式的浅蓝色
        cv2.line(im0, right_shoulder, right_hip, color_light_blue, 4)
        cv2.line(im0, right_hip, right_knee, color_light_blue, 4)
        cv2.line(im0, right_knee, right_ankle, color_light_blue, 4)
        cv2.line(im0, right_shoulder, right_elbow, color_light_blue, 4)
        cv2.line(im0, right_elbow, right_wrist, color_light_blue, 4)
        
        # 绘制关节点
        color_yellow = (0, 255, 255)  # BGR格式的黄色
        cv2.circle(im0, right_shoulder, 7, color_yellow, -1)
        cv2.circle(im0, right_hip, 7, color_yellow, -1)
        cv2.circle(im0, right_knee, 7, color_yellow, -1)
        cv2.circle(im0, right_ankle, 7, color_yellow, -1)
        cv2.circle(im0, right_elbow, 7, color_yellow, -1)
        cv2.circle(im0, right_wrist, 7, color_yellow, -1)

        # 绘制角度指示器
        # 肩膀-髋部角度指示器（垂直方向）
        if shoulder_hip_vertical_angle is not None:
            cv2.ellipse(im0, right_hip, (30, 30), angle=0, startAngle=-90, 
                       endAngle=-90-shoulder_hip_vertical_angle, color=(255, 255, 255), thickness=3)
        
        # 膝盖-踝关节角度指示器（垂直方向）
        if knee_ankle_vertical_angle is not None:
            cv2.ellipse(im0, right_knee, (20, 20), angle=0, startAngle=-90, 
                       endAngle=-90+knee_ankle_vertical_angle, color=(255, 255, 255), thickness=3)

        # 绘制计数
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['DEADLIFT_COUNT']),
            pos=(int(frame_width * 0.68), 135),
            text_color=(255, 255, 115),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )
        
        draw_text(
            im0,
            "INCORRECT: " + str(self.state_tracker['IMPROPER_DEADLIFT']),
            pos=(int(frame_width * 0.68), 180),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(221, 0, 0),
        )
        
        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            "ANGLE: " + str(round(shoulder_hip_knee_angle, 2)),
            pos=(int(frame_width * 0.68), 225),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )
        
        # 获取当前状态
        current_state = self._get_state(shoulder_hip_knee_angle)
        self.state_tracker['curr_state'] = current_state
        self._update_state_sequence(current_state)

        # 硬拉计数逻辑
        if current_state == 's1':
            if len(self.state_tracker['state_seq']) == 3 and not self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['DEADLIFT_COUNT'] += 1
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
            elif 's2' in self.state_tracker['state_seq'] and len(self.state_tracker['state_seq']) == 1:
                self.state_tracker['IMPROPER_DEADLIFT'] += 1
            elif self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['IMPROPER_DEADLIFT'] += 1
            self.state_tracker['state_seq'] = []
            self.state_tracker['INCORRECT_POSTURE'] = False
        # 反馈显示逻辑
        else:
            # 检查各种错误姿势
            if shoulder_hip_vertical_angle > self.thresholds['HIP_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][0] = True
                
            if knee_ankle_vertical_angle > self.thresholds['KNEE_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][1] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            if arm_angle < self.thresholds['ARM_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][2] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            self.state_tracker['COUNT_FRAMES'][self.state_tracker['DISPLAY_TEXT']] += 1
            
            # 显示错误提示
            im0 = _show_feedback(im0, self.state_tracker['COUNT_FRAMES'], self.FEEDBACK_ID_MAP)
            
        # 重置显示文本
        self.state_tracker['DISPLAY_TEXT'][self.state_tracker['COUNT_FRAMES'] > self.thresholds['CNT_FRAME_THRESH']] = False
        self.state_tracker['COUNT_FRAMES'][self.state_tracker['COUNT_FRAMES'] > self.thresholds['CNT_FRAME_THRESH']] = 0    
        
        # 记录事件
        self._record_events(count[0], fps)
        
        # 准备返回数据 - 添加详细的错误统计
        result_data = {
            'processed_frame': im0,
            'correct_count': self.state_tracker['DEADLIFT_COUNT'],
            'incorrect_count': self.state_tracker['IMPROPER_DEADLIFT'],
            'total_count': self.state_tracker['DEADLIFT_COUNT'] + self.state_tracker['IMPROPER_DEADLIFT'],
            'events': self.events[-10:] if self.events else [],  # 只返回最近10个事件
            'error_summary': self._get_error_summary(),  # 添加错误摘要
            'deadlift_details': self._get_deadlift_details()   # 添加硬拉详情
        }
        
        return result_data

    def _get_error_summary(self):
        """获取错误统计摘要"""
        error_summary = {}
        for event in self.events:
            if event['type'] in ['DEADLIFT_COMPLETE_INCORRECT']:
                for error in event.get('error_reasons', []):
                    error_code = error.get('code')
                    error_summary[error_code] = error_summary.get(error_code, 0) + 1
        
        return error_summary

    def _get_deadlift_details(self):
        """获取每个硬拉的详细信息"""
        deadlift_details = []
        for event in self.events:
            if event['type'] in ['DEADLIFT_COMPLETE_CORRECT', 'DEADLIFT_COMPLETE_INCORRECT']:
                deadlift_details.append({
                    'deadlift_number': event.get('deadlift_number', 0),
                    'type': '正确' if event['type'] == 'DEADLIFT_COMPLETE_CORRECT' else '错误',
                    'frame': event.get('frame', 0),
                    'timestamp': event.get('timestamp', 0),
                    'duration_frames': event.get('duration_frames', 0),
                    'errors': event.get('errors_during_deadlift', []),
                    'error_reasons': event.get('error_reasons', [])
                })
        
        # 按硬拉编号排序
        deadlift_details.sort(key=lambda x: x['deadlift_number'])
        return deadlift_details