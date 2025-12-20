// Mock data for the application

// Classes datal
export const classes = [
  {
    id: 1,
    name: '高一体育',
    studentCount: 45,
    teacher: '张老师'
  },
  {
    id: 2,
    name: '高二体育',
    studentCount: 42,
    teacher: '李老师'
  },
  {
    id: 3,
    name: '高三体育',
    studentCount: 38,
    teacher: '王老师'
  }
];

// Course codes data - these would typically be stored in a database
export const courseCodes = [
  {
    id: 1,
    code: 'SP2024A',
    classId: 1,
    createdAt: '2024-05-10T08:30:00Z',
    expiresAt: '2024-05-31T23:59:59Z',
    used: false
  },
  {
    id: 2,
    code: 'SP2024B',
    classId: 2,
    createdAt: '2024-05-11T10:15:00Z',
    expiresAt: '2024-05-31T23:59:59Z',
    used: false
  }
];

// Generate a random course code
const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'; // Exclude similar looking characters

export const generateCourseCode = (length = 6) => {
  let result = '';
  const charsLength = chars.length;
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * charsLength));
  }
  return result;
};

// Save course code to localStorage (simulating database storage)
export const saveCourseCode = (courseCode) => {
  const existingCodes = getCourseCodes();
  const newCode = {
    id: existingCodes.length + 1,
    ...courseCode,
    createdAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(), // 30 days expiration
    used: false
  };
  existingCodes.push(newCode);
  localStorage.setItem('courseCodes', JSON.stringify(existingCodes));
  return newCode;
};

// Get all course codes from localStorage
export const getCourseCodes = () => {
  const codes = localStorage.getItem('courseCodes');
  return codes ? JSON.parse(codes) : [...courseCodes]; // Fallback to mock data if no codes in localStorage
};

// Get course code by code string
export const getCourseCodeByCode = (code) => {
  const codes = getCourseCodes();
  return codes.find(c => c.code === code);
};

// Validate course code
export const validateCourseCode = (code) => {
  const courseCode = getCourseCodeByCode(code);
  if (!courseCode) {
    return { valid: false, message: '课程码不存在' };
  }

  const now = new Date();
  const expiresAt = new Date(courseCode.expiresAt);

  if (now > expiresAt) {
    return { valid: false, message: '课程码已过期' };
  }

  // For simplicity, we'll allow multiple uses. In a real app, you might want to track usage.
  // if (courseCode.used) {
  //   return { valid: false, message: '课程码已使用' };
  // }

  return { valid: true, courseCode };
};

// Add student to class (simulated)
export const addStudentToClass = (studentId, className) => {
  // In a real app, this would update the database
  console.log(`Adding student ${studentId} to class ${className}`);
  return true;
};

// Assignments data
export const assignments = [
  {
    id: 1,
    title: '跑步训练',
    description: '完成5公里跑步',
    dueDate: '2024-05-15',
    classId: 1,
    teacherId: 1,
    subject: '体育',
    status: 'published'
  },
  {
    id: 2,
    title: '篮球技巧',
    description: '练习投篮和运球',
    dueDate: '2024-05-20',
    classId: 1,
    teacherId: 1,
    subject: '体育',
    status: 'published'
  },
  {
    id: 3,
    title: '游泳训练',
    description: '完成1000米自由泳',
    dueDate: '2024-05-25',
    classId: 2,
    teacherId: 2,
    subject: '体育',
    status: 'published'
  }
];

// Submissions data
export const submissions = [
  {
    id: 1,
    assignmentId: 1,
    studentId: 1,
    studentName: '小明',
    content: '已完成5公里跑步，用时25分钟',
    submissionDate: '2024-05-10',
    score: 95,
    feedback: '优秀',
    classId: 1,
    status: 'graded'
  },
  {
    id: 2,
    assignmentId: 1,
    studentId: 2,
    studentName: '小红',
    content: '已完成5公里跑步，用时28分钟',
    submissionDate: '2024-05-12',
    score: 88,
    feedback: '良好',
    classId: 1,
    status: 'graded'
  },
  {
    id: 3,
    assignmentId: 2,
    studentId: 1,
    studentName: '小明',
    content: '已完成投篮练习，命中30个',
    submissionDate: '2024-05-18',
    score: 90,
    feedback: '优秀',
    classId: 1,
    status: 'graded'
  },
  {
    id: 4,
    assignmentId: 2,
    studentId: 2,
    studentName: '小红',
    content: '已完成投篮练习，命中25个',
    submissionDate: '2024-05-19',
    score: 85,
    feedback: '良好',
    classId: 1,
    status: 'graded'
  },
  {
    id: 5,
    assignmentId: 3,
    studentId: 3,
    studentName: '小刚',
    content: '已完成1000米自由泳，用时15分钟',
    submissionDate: '2024-05-22',
    score: 92,
    feedback: '优秀',
    classId: 2,
    status: 'graded'
  }
];

// Assignments Submissions data (for TeacherAssignments.vue)
export const assignmentsSubmissions = [
  {
    assignmentId: 1,
    title: '跑步训练',
    dueDate: '2024-05-15',
    totalSubmissions: 30,
    pendingSubmissions: 5,
    gradedSubmissions: 25
  },
  {
    assignmentId: 2,
    title: '篮球技巧',
    dueDate: '2024-05-20',
    totalSubmissions: 28,
    pendingSubmissions: 8,
    gradedSubmissions: 20
  },
  {
    assignmentId: 3,
    title: '游泳训练',
    dueDate: '2024-05-25',
    totalSubmissions: 22,
    pendingSubmissions: 12,
    gradedSubmissions: 10
  }
];

// Courses data
export const courses = [
  {
    id: 'PE101',
    name: '体能训练课程',
    description: '学习和掌握各种体能训练技巧，包括俯卧撑、仰卧起坐等基础动作',
    subject: '体育',
    status: '进行中',
    classId: 1,
    teacherId: 1,
    assignments: [
      {
        id: 1,
        title: '50米折返跑',
        description: '完成50米折返跑测试，记录时间',
        deadline: '2024-06-20T23:59:59',
        create_time: '2024-06-10T10:30:00',
        course_id: 'PE101',
        subject: '体能测试',
        status: '进行中',
        points: 100
      },
      {
        id: 2,
        title: '仰卧起坐',
        description: '完成1分钟仰卧起坐测试，记录数量',
        deadline: '2024-06-25T23:59:59',
        create_time: '2024-06-15T14:20:00',
        course_id: 'PE101',
        subject: '体能测试',
        status: '进行中',
        points: 100
      }
    ]
  },
  {
    id: 'PE202',
    name: '协调能力训练',
    description: '通过跳绳、敏捷梯等训练提升身体协调性和反应能力',
    subject: '体育',
    status: '已完成',
    classId: 1,
    teacherId: 1,
    assignments: [
      {
        id: 3,
        title: '跳绳技巧练习',
        description: '掌握基本跳绳技巧，提高协调性和耐力',
        deadline: '2024-01-15T23:59:59',
        create_time: '2024-01-05T14:20:00',
        course_id: 'PE202',
        subject: '协调训练',
        status: '已完成',
        points: 100
      }
    ]
  },
  {
    id: 'PE303',
    name: '田径基础训练',
    description: '学习田径运动的基本技能，包括短跑、跳远等项目',
    subject: '体育',
    status: '进行中',
    classId: 2,
    teacherId: 2,
    assignments: [
      {
        id: 4,
        title: '50米短跑测试',
        description: '进行50米短跑测试，记录成绩',
        deadline: '2024-07-10T23:59:59',
        create_time: '2024-06-20T09:00:00',
        course_id: 'PE303',
        subject: '田径',
        status: '进行中',
        points: 100
      }
    ]
  }
];

// Teaching videos data
export const teachingVideos = [
  {
    id: 1,
    title: '跑步姿势讲解',
    description: '正确的跑步姿势教学',
    duration: '15:30',
    views: 1200,
    uploadDate: '2024-04-10',
    url: 'https://example.com/video1'
  },
  {
    id: 2,
    title: '篮球投篮技巧',
    description: '三步上篮和跳投技巧',
    duration: '20:15',
    views: 950,
    uploadDate: '2024-04-15',
    url: 'https://example.com/video2'
  },
  {
    id: 3,
    title: '游泳呼吸法',
    description: '自由泳和蛙泳的呼吸技巧',
    duration: '18:45',
    views: 820,
    uploadDate: '2024-04-20',
    url: 'https://example.com/video3'
  }
];
