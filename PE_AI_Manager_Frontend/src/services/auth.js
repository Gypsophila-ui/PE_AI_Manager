import apiClient from './axios';
import SHA256 from 'crypto-js/sha256';

// API错误处理函数
const handleApiError = (error, operationType) => {
  if (error.response) {
    // 服务器返回错误状态码
    const errorCode = error.response.data?.error_code || -99;

    // 只处理特定的用户友好错误
    const specificErrors = {
      '-10': {
        password_change: '参数错误'
      },
      '-11': {
        password_change: '查询id是否存在的sql操作执行失败'
      },
      '-12': {
        password_change: '查询id对应密码的sql操作执行失败'
      },
      '-13': {
        password_change: '修改用户密码的sql操作失败'
      },
      '-21': {
        login: '用户不存在',
        register: '该用户已经注册过账号',
        password_change: '账号不存在'
      },
      '-22': {
        login: '账号无效',
        register: '该ID不在基准库内，无效',
        password_change: '账号信息异常'
      },
      '-23': {
        login: '登录密码输入错误',
        register: '密码格式错误',
        password_change: '输入的旧密码不正确'
      }
    };

    // 检查是否有特定错误信息
    if (specificErrors[errorCode] && specificErrors[errorCode][operationType]) {
      return {
        success: false,
        error_code: errorCode,
        message: specificErrors[errorCode][operationType]
      };
    }

    // 其他错误返回通用提示
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: errorCode, message: genericMessage };
  } else if (error.request) {
    // 请求已发送但没有收到响应
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: -99, message: genericMessage };
  } else {
    // 请求配置出错
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: -99, message: genericMessage };
  }
};

// SHA-256加密函数（使用crypto-js）
const sha256 = async (str) => {
  return SHA256(str).toString();
};

// 教师登录
export const loginTeacher = async (teacher_id, password) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const requestData = {
      first: teacher_id,
      second: passwordHash
    };

    console.log('📤 发送教师登录请求:', {
      url: '/User/login_teacher',
      teacher_id,
      passwordHash: passwordHash.substring(0, 20) + '...' // 只显示部分哈希值
    });

    const response = await apiClient.post('/User/login_teacher', requestData);

    console.log('✅ 教师登录成功:', response.data);

    const loginData = response.data;

    if (loginData) {
      localStorage.setItem('token', loginData.data);
    }

    return { success: true, data: loginData };
  } catch (error) {
    console.error('❌ 教师登录失败:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      message: error.message
    });
    return handleApiError(error, 'login');
  }
};

// 学生登录
export const loginStudent = async (student_id, password) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const requestData = {
      first: student_id,
      second: passwordHash
    };

    console.log('📤 发送学生登录请求:', {
      url: '/User/login_student',
      student_id,
      passwordHash: passwordHash.substring(0, 20) + '...' // 只显示部分哈希值
    });

    const response = await apiClient.post('/User/login_student', requestData);

    console.log('✅ 学生登录成功:', response.data);

    const loginData = response.data;

    return { success: true, data: loginData };
  } catch (error) {
    console.error('❌ 学生登录失败:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      message: error.message
    });
    return handleApiError(error, 'login');
  }
};

// 教师注册
export const registerTeacher = async (id, password, name, gender, title, college, department) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const response = await apiClient.post('/User/new_teacher', {
      first: id,
      second: passwordHash,
      third: name,
      fourth: gender,
      fifth: title,
      sixth: college,
      seventh: department
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'register');
  }
};

// 学生注册
export const registerStudent = async (id, password, name, gender, major, college, department) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const requestData = {
      first: id,
      second: passwordHash,
      third: name,
      fourth: gender,
      fifth: major,
      sixth: college,
      seventh: department
    };

    console.log('📤 发送学生注册请求:', {
      url: '/User/new_student',
      data: requestData
    });

    const response = await apiClient.post('/User/new_student', requestData);

    console.log('✅ 学生注册成功:', response.data);
    return { success: true, data: response.data };
  } catch (error) {
    console.error('❌ 学生注册失败:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      message: error.message
    });
    return handleApiError(error, 'register');
  }
};

// 教师修改密码
export const changeTeacherPassword = async (id, oldPassword, newPassword) => {
  try {
    // 将密码转换为SHA-256
    const oldPasswordHash = await sha256(oldPassword);
    const newPasswordHash = await sha256(newPassword);

    const response = await apiClient.post('/User/change_teacher_password', {
      first: id,
      second: oldPasswordHash,
      third: newPasswordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'password_change');
  }
};

// 学生修改密码
export const changeStudentPassword = async (id, oldPassword, newPassword) => {
  try {
    // 将密码转换为SHA-256
    const oldPasswordHash = await sha256(oldPassword);
    const newPasswordHash = await sha256(newPassword);

    const response = await apiClient.post('/User/change_student_password', {
      first: id,
      second: oldPasswordHash,
      third: newPasswordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'password_change');
  }
};

export default {
  loginTeacher,
  loginStudent,
  registerTeacher,
  registerStudent,
  changeTeacherPassword,
  changeStudentPassword
};
