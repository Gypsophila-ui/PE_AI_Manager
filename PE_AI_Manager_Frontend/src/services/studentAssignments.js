import { apiClient, aiClient } from './axios';

/**
 * 学生作业相关服务
 */
export class StudentAssignmentService {
  constructor() {
    this.BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://127.0.0.1:8000';
  }

  /**
   * 获取作业详情
   */
  async fetchAssignmentDetails(courseId, assignmentId) {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = user.token;

      if (!token) {
        throw new Error('未找到认证token，请重新登录');
      }

      const response = await apiClient.post('/Homework/get_info_by_homework_id', {
        first: courseId,
        second: assignmentId
      });

      if (response.data.success) {
        const homeworkDataArray = response.data.data.split('\t\r');

        if (homeworkDataArray.length >= 4) {
          const [
            title,         // 作业标题
            description,   // 作业描述
            deadline,      // 截至时间
            createTime    // 创建时间
          ] = homeworkDataArray;

          return {
            id: assignmentId,
            title: title || `作业 ${assignmentId}`,
            description: description || '暂无描述',
            deadline: deadline || '待定',
            create_time: createTime || '',
            course_id: courseId,
            subject: '体育',
            status: new Date(deadline) > new Date() ? '进行中' : '已截止',
            points: 100
          };
        } else {
          throw new Error('作业数据格式不正确');
        }
      } else {
        throw new Error(response.data.message || '获取作业详情失败');
      }
    } catch (err) {
      console.error('获取作业详情失败:', err);
      throw err;
    }
  }

  /**
   * 获取最终得分
   */
  async fetchFinalScore(courseId, assignmentId) {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const studentId = user.id;
      const jwt = user.token;

      if (!studentId || !jwt) {
        console.log('未找到用户信息，跳过获取最终得分');
        return null;
      }

      // 根据课程ID获取教师ID
      let teacherId = '';
      try {
        const courseResponse = await apiClient.post('/Course/get_course_info', {
          first: courseId,
          second: jwt
        });

        if (courseResponse.data.success && courseResponse.data.data) {
          teacherId = courseResponse.data.data.teacher_id || '';
        } else {
          console.error('获取课程信息失败:', courseResponse.data);
        }
      } catch (error) {
        console.error('获取课程信息时发生错误:', error);
      }

      // 调用get_final_score接口
      const response = await apiClient.post('/Homework/get_final_score', {
        first: '0', // 0为学生身份
        second: teacherId,
        third: jwt,
        fourth: studentId,
        fifth: assignmentId
      });

      console.log('获取最终得分响应:', response.data);

      // 检查返回值
      if (response.data && typeof response.data === 'number') {
        if (response.data >= 0) {
          return response.data;
        } else if (response.data === -26) {
          // 当前学生在当前作业下还没有已经进行评分的提交
          return null;
        } else {
          console.warn('获取最终得分返回错误码:', response.data);
          return null;
        }
      } else if (response.data && typeof response.data === 'object' && response.data.score !== undefined) {
        return response.data.score;
      } else {
        console.warn('获取最终得分返回数据格式不正确:', response.data);
        return null;
      }
    } catch (err) {
      console.error('获取最终得分失败:', err);
      return null;
    }
  }

  /**
   * 获取AI类型（动作类型）
   */
  async getPoseType(assignmentId) {
    try {
      const aiTypeResponse = await apiClient.post('/Homework/get_AI_type', {
        first: assignmentId
      });
      if (aiTypeResponse.success) {
        const poseType = aiTypeResponse.data.data || 'squat';
        console.log('获取到的动作类型:', poseType);
        return poseType;
      } else {
        console.warn('获取AI类型失败，使用默认动作类型: squat');
        return 'squat';
      }
    } catch (error) {
      console.error('获取AI类型失败:', error);
      return 'squat';
    }
  }

  /**
   * 获取当前学生ID
   */
  getStudentId() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    return user.id || 'student1';
  }

  /**
   * 创建视频上传的FormData
   */
  createVideoFormData(selectedFile) {
    const formData = new FormData();
    formData.append('file', selectedFile);
    return formData;
  }

  /**
   * 构造视频处理URL
   */
  buildVideoProcessingUrl(assignmentId, studentId, poseType) {
    return `${this.BASE_URL}/process_and_save_video?homework_id=${encodeURIComponent(assignmentId)}&student_id=${encodeURIComponent(studentId)}&pose_type=${encodeURIComponent(poseType)}`;
  }

  /**
   * 处理SSE事件
   */
  handleSSEEvent(data, assignmentId, studentId, poseType, aiResult, processedVideoUrlValue) {
    switch (data.event) {
      case 'init':
        break;

      case 'frame':
        break;

      case 'final_stats':
        if (data.data.download_url) {
          let downloadUrl = data.data.download_url;
          if (!downloadUrl.startsWith('http')) {
            downloadUrl = `${this.BASE_URL}${downloadUrl}`;
          }

          const playbackUrl = `${this.BASE_URL}/get_processed_video?homework_id=${assignmentId}&student_id=${studentId}`;

          processedVideoUrlValue.value = playbackUrl;

          aiResult.value = {
            final_count: data.data.max_count,
            processed_frame_count: data.data.processed_frame_count,
            total_time: data.data.total_time,
            video_url: playbackUrl || data.data.download_url || ''
          };
        }
        break;

      case 'error':
        throw new Error(data.data.message);
    }
  }

  /**
   * 获取AI反馈
   */
  async fetchAIFeedback(assignmentId, studentId, poseType, aiResult) {
    try {
      const response = await fetch(`${this.BASE_URL}/query_records?homework_id=${assignmentId}&student_id=${studentId}&pose_type=${poseType}`);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const records = await response.json();
      if (records && records.length > 0) {
        const latestRecord = records[records.length - 1];
        if (latestRecord && latestRecord.feedback_json) {
          const feedbackData = JSON.parse(latestRecord.feedback_json);

          const totalCount = latestRecord.total_count || 0;
          const correctCount = latestRecord.correct_count || 0;
          const incorrectCount = latestRecord.incorrect_count || 0;
          const accuracyRate = feedbackData.performance?.accuracy_rate?.toFixed(2) || 0;
          const videoDuration = latestRecord.video_duration?.toFixed(2) || 0;

          aiResult.value.AI_feedback = `本次动作共完成${totalCount}次，其中${correctCount}次动作标准，${incorrectCount}次动作不标准，标准度为${accuracyRate}%，视频持续时长${videoDuration}秒。`;
          console.log('现在的AI_feedback:', aiResult.value);
        }
      }
    } catch (error) {
      console.error('获取反馈记录失败:', error);
      aiResult.value.AI_feedback = 'AI反馈获取失败，等待教师批改';
    }
  }

  /**
   * 处理SSE流
   */
  async processSSEStream(reader, decoder, assignmentId, studentId, poseType, resolve, reject) {
    let buffer = '';
    let videoChunks = [];
    const aiResult = { value: null };
    const processedVideoUrlValue = { value: null };

    const processStream = () => {
      reader.read().then(({done, value}) => {
        if (done) {
          console.log('视频处理完成');

          if (videoChunks.length > 0) {
            const processedVideoBlobValue = new Blob(videoChunks, { type: 'video/mp4' });
            processedVideoUrlValue.value = URL.createObjectURL(processedVideoBlobValue);
          }

          resolve({
            processedVideoUrlValue: processedVideoUrlValue.value,
            aiResult: aiResult.value
          });
          return;
        }

        buffer += decoder.decode(value, {stream: true});

        let lines = buffer.split('\n\n');
        buffer = lines.pop();

        for (const chunk of lines) {
          if (chunk.startsWith('data: ')) {
            try {
              const jsonData = chunk.slice(6);
              const data = JSON.parse(jsonData);

              this.handleSSEEvent(data, assignmentId, studentId, poseType, aiResult, processedVideoUrlValue);

              if (data.event === 'final_stats') {
                this.fetchAIFeedback(assignmentId, studentId, poseType, aiResult);
              }
            } catch (e) {
              videoChunks.push(value);
            }
          } else {
            videoChunks.push(value);
          }
        }

        processStream();
      }).catch(error => {
        console.error('Error:', error);
        reject(error);
      });
    };

    processStream();
  }

  /**
   * 提交作业
   */
  async submitAssignment(courseId, assignmentId, selectedFile, processedVideoBlob, processedVideoUrl) {
    return new Promise(async (resolve, reject) => {
      try {
        const formData = this.createVideoFormData(selectedFile);
        const poseType = await this.getPoseType(assignmentId);
        const studentId = this.getStudentId();
        const url = this.buildVideoProcessingUrl(assignmentId, studentId, poseType);

        console.log('开始上传视频到AI后端服务...');

        const response = await fetch(url, {
          method: 'POST',
          body: formData
        });

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        await this.processSSEStream(reader, decoder, assignmentId, studentId, poseType, resolve, reject);
      } catch (error) {
        console.error('作业提交失败:', error);
        reject(error);
      }
    });
  }

  /**
   * 保存作业提交信息
   */
  async saveAssignmentSubmission(courseId, assignmentId, aiResult, studentId, processedVideoUrlValue) {
    try {
      console.log('开始保存作业提交信息...');
      console.log('aiResult:', aiResult);
      console.log('processedVideoUrlValue:', processedVideoUrlValue);

      // 获取JWT token
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('未找到认证token，请重新登录');
      }

      // 准备视频URL - 优先使用AI处理后的视频URL，如果没有则使用原始视频
      let videoUrl = processedVideoUrlValue;
      // 如果videoUrl是相对路径或需要转换为get_processed_video格式，确保使用绝对路径
      if (videoUrl && !videoUrl.startsWith('http') && assignmentId && studentId) {
        videoUrl = `${this.BASE_URL}/get_processed_video?homework_id=${assignmentId}&student_id=${studentId}`;
      }
      if (!videoUrl && processedVideoBlob) {
        // 如果没有AI返回的视频URL但有Blob，创建临时URL
        videoUrl = URL.createObjectURL(processedVideoBlob);
      }

      // 如果仍然没有视频URL，使用原始视频文件的URL
      if (!videoUrl && selectedFile) {
        videoUrl = URL.createObjectURL(selectedFile);
      }

      // 根据API文档构造请求参数
      const submitData = {
        first: studentId,
        second: token,
        third: courseId,
        fourth: assignmentId.toString(),
        fifth: videoUrl
      };

      console.log('提交作业参数:', submitData);

      // 调用submit_homework接口
      const submitResponse = await apiClient.post('/Homework/submit_homework', submitData, {
        headers: {
          'Content-Type': 'application/json'
        }
      });

      // 上传成功处理
      if (submitResponse.data.success) {
        console.log('作业提交成功:', submitResponse.data);

        // 获取submit_id - 从响应数据中提取
        let submitId = null;
        if (submitResponse.data && submitResponse.data.data) {
          submitId = submitResponse.data.data;
        } else if (submitResponse.data && typeof submitResponse.data === 'string') {
          submitId = submitResponse.data;
        }

        console.log('获取到的submit_id:', submitId);

        // 如果有submit_id，调用AI_test API保存AI评价结果
        if (submitId) {
          await this.saveAIEvaluation(submitId, aiResult, studentId);
        }

        // 根据AI评价结果显示不同的提示信息
        if (aiResult && aiResult.video_url) {
          console.log(`作业提交成功！视频已处理完成。\n可在作业详情查看提交记录。`);
        } else {
          console.log(`作业提交成功！\nAI评价暂不可用，等待教师批改。\n可在作业详情查看提交记录。`);
        }

        // 更新作业状态为已完成
        if (assignment) {
          assignment.status = '已完成';
        }
      } else {
        console.error('作业提交失败:', submitResponse.data);
        throw new Error(submitResponse.data.message || '作业提交失败');
      }
    } catch (error) {
      console.error('保存作业提交信息失败:', error);
      // 即使作业提交失败，也不影响视频处理的成功
      // 只记录错误，不抛出异常
      if (error.response) {
        console.error('错误响应:', error.response.data);
        const errorMsg = error.response.data?.message || '作业提交记录保存失败，请稍后重试。';
        console.error(errorMsg);
      } else {
        console.error('作业提交记录保存失败，请稍后重试。');
      }
    }
  }

  /**
   * 保存AI评价结果
   */
  async saveAIEvaluation(submitId, aiResult, studentId) {
    try {
      console.log('开始保存AI评价结果...');
      console.log('submit_id:', submitId);
      console.log('aiResult:', aiResult);

      // 获取JWT token
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('未找到认证token，请重新登录');
      }

      // 准备视频URL - 优先使用AI处理后的视频URL
      let videoUrl = aiResult.video_url;
      // 如果是相对路径，转换为绝对路径
      if (videoUrl && !videoUrl.startsWith('http')) {
        videoUrl = `${this.BASE_URL}${videoUrl}`;
      }
      if (!videoUrl && processedVideoBlob) {
        videoUrl = URL.createObjectURL(processedVideoBlob);
      }

      // 如果仍然没有视频URL，使用原始视频文件的URL
      if (!videoUrl && selectedFile) {
        videoUrl = URL.createObjectURL(selectedFile);
      }

      // 准备AI评价数据 - 处理空的AI评价结果
      // 支持两种aiResult格式：一种是AI处理返回的格式（final_count等），一种是AI评价API需要的格式（score等）
      const aiEvaluationData = {
        first: submitId,
        second: videoUrl,
        third: aiResult.score || aiResult.final_count || '0',
        fourth: aiResult.AI_feedback || 'AI评价暂不可用，等待教师批改'
      };

      console.log('发送到AI_test的数据:', aiEvaluationData);

      // 调用AI_test接口
      const aiTestResponse = await apiClient.post('/Homework/AI_test', aiEvaluationData, {
        headers: {
          'Content-Type': 'application/json'
        }
      });

      console.log('AI_test API响应数据:', aiTestResponse.data);

      // 处理API返回结果
      if (aiTestResponse.data.success) {
        const result = aiTestResponse.data;
        console.log('AI评价保存成功:', result);
      } else {
        console.error('AI_test API返回异常状态码:', aiTestResponse.status);
      }
    } catch (error) {
      console.error('保存AI评价结果失败:', error);
      // 即使AI评价保存失败，也不影响作业提交的成功
      // 只记录错误，不抛出异常
      if (error.response) {
        console.error('错误响应:', error.response.data);
        const errorCode = error.response.data?.code || error.response.status;
        this.handleAIError(errorCode);
      }
    }
  }

  /**
   * 处理AI评价API错误码
   */
  handleAIError(code) {
    let errorMessage = '';
    switch (code) {
      case -1:
        errorMessage = '参数错误';
        break;
      case -11:
        errorMessage = '查询提交记录存在性的SQL操作无法执行';
        break;
      case -12:
        errorMessage = '修改评价的SQL操作无法正常执行';
        break;
      case -21:
        errorMessage = '当前提交记录不存在';
        break;
      case -99:
        errorMessage = '意料之外的错误';
        break;
      default:
        errorMessage = `未知错误码: ${code}`;
    }
    console.error('AI评价错误:', errorMessage);
  }

  /**
   * 获取处理后的视频
   */
  async getProcessedVideo(homeworkId, studentId) {
    try {
      console.log('开始获取处理后的视频...');

      // 构建请求URL
      const url = `${this.BASE_URL}/get_processed_video`;

      const response = await fetch(url + `?homework_id=${homeworkId}&student_id=${studentId}`);

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      // 将响应转换为Blob
      const videoBlob = await response.blob();
      if (response.status === 200) {
        // 创建视频 URL
        const videoUrl = URL.createObjectURL(videoBlob);
        console.log('成功获取处理后的视频');

        return {
          videoUrl,
          videoBlob
        };
      } else if (response.status === 404) {
        console.log('未找到处理后的视频');
        return null;
      }
    } catch (error) {
      console.error('获取处理后的视频失败:', error);
      throw error;
    }
  }

  /**
   * 下载处理后的视频
   */
  async downloadProcessedVideo(processedVideoUrl, downloadVideoUrl, processedVideoBlob, assignmentId) {
    let downloadUrl = downloadVideoUrl;

    // 如果没有下载URL但有Blob，创建新的URL
    if (!downloadUrl && processedVideoBlob) {
      downloadUrl = URL.createObjectURL(processedVideoBlob);
    }

    // 如果仍然没有URL，尝试从API获取
    if (!downloadUrl) {
      try {
        // 获取当前用户信息
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const studentId = user.id || 'student1';

        // 调用get_processed_video API
        const videoResult = await this.getProcessedVideo(assignmentId, studentId);
        if (videoResult && videoResult.videoUrl) {
          downloadUrl = videoResult.videoUrl;
        }
      } catch (error) {
        console.error('获取处理后的视频用于下载失败:', error);
        throw new Error('无法获取视频文件进行下载');
      }
    }

    if (!downloadUrl) {
      throw new Error('没有可用的视频文件进行下载');
    }

    // 创建下载链接
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.download = `processed_video_${assignmentId}_${new Date().getTime()}.mp4`;

    // 触发下载
    document.body.appendChild(link);
    link.click();

    // 清理
    document.body.removeChild(link);

    // 如果是临时创建的URL，清理它
    if (!processedVideoUrl && downloadUrl && downloadUrl.startsWith('blob:')) {
      setTimeout(() => {
        URL.revokeObjectURL(downloadUrl);
      }, 1000);
    }

    console.log('视频下载已触发');
  }
}
