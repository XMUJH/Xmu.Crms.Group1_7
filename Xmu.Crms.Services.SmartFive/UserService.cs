﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.SmartFive
{
    public class UserService : IUserService
    {
        private CrmsContext _db;

        // 在构造函数里添加依赖的Service（参考模块标准组的类图）
        public UserService(CrmsContext db)
        {
            _db = db;
        }

        public UserInfo GetUserByUserId(long userId)//测试成功
        {
            if (userId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from user in _db.UserInfo
                     where user.Id == userId
                     select new UserInfo
                     {
                         Id = user.Id,
                         Phone = user.Phone,
                         Avatar = user.Avatar,
                         Password = user.Password,
                         Name = user.Name,
                         SchoolId = user.SchoolId,
                         Gender = user.Gender,
                         Type = user.Type,
                         Number = user.Number,
                         Education = user.Education,
                         Title = user.Title,
                         Email = user.Email
                     }).SingleOrDefault();
            
            if (u == null)
            {
                throw new UserNotFoundException();
            }

            return u;
        }

        public long InsertAttendanceById(long classId, long seminarId, long userId, double longitude, double latitude)//测试成功时间
        {
            var location = _db.Location.Include(c => c.Seminar).Include(c => c.ClassInfo).SingleOrDefault(c => c.ClassInfoId == classId && c.SeminarId == seminarId);
            if (classId <= 0 || seminarId <= 0 || userId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from class1 in _db.ClassInfo
                     where class1.Id == classId
                     select class1).SingleOrDefault();
            if (u == null)
                throw new ClassNotFoundException();
            var v = (from seminar in _db.Seminar
                     where seminar.Id == seminarId
                     select seminar).SingleOrDefault();
            if (v == null)
                throw new SeminarNotFoundException();
            if (longitude - (double)location.Longitude>0.5 || longitude - (double)location.Longitude < -0.5 || latitude - (double)location.Latitude > 0.5 || latitude - (double)location.Latitude < -0.5)
            {
                //throw new Shared.Exceptions.InvalidOperationException("超出签到范围");
                throw new UserNotFoundException("超出签到范围");
            }
            Attendance attendance = new Attendance();
            IList<Attendance> attendanceList = this.ListAttendanceById(classId, seminarId);
            int cnt = attendanceList.Count;
            attendance.ClassInfo = (from i in _db.ClassInfo
                                    where i.Id == classId
                                    select i).SingleOrDefault();
            attendance.Student = (from j in _db.UserInfo
                                  where j.Id == userId
                                  select j).SingleOrDefault();
            attendance.Seminar = (from k in _db.Seminar
                                  where k.Id == seminarId
                                  select k).SingleOrDefault();
            if(location.Status == 1)
                attendance.AttendanceStatus = AttendanceStatus.Present;
            if (location.Status == 0)
                attendance.AttendanceStatus = AttendanceStatus.Late;
            _db.Attendences.Add(attendance);
            _db.SaveChanges();
            return (attendance.AttendanceStatus == AttendanceStatus.Present) ? 0 : 1;
            throw new System.InvalidOperationException();
        }

        public IList<UserInfo> ListAbsenceStudent(long seminarId, long classId) //测试成功
        {
            if (classId <= 0 || seminarId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from class1 in _db.ClassInfo
                     where class1.Id == classId
                     select class1).SingleOrDefault();
            if (u == null)
                throw new ClassNotFoundException();
            var v = (from seminar in _db.Seminar
                     where seminar.Id == seminarId
                     select seminar).SingleOrDefault();
            if (v == null)
                throw new SeminarNotFoundException();
            var u1 = (from user in _db.Attendences//.Include(a => a.Student)user.Student
                      where user.AttendanceStatus == AttendanceStatus.Absent && user.Seminar.Id == seminarId && user.ClassInfo.Id == classId
                      select new UserInfo
                     {
                         Id = user.Student.Id,
                         Phone = user.Student.Phone,
                         Avatar = user.Student.Avatar,
                         Password = user.Student.Password,
                         Name = user.Student.Name,
                         School = new School { Id = user.Student.School.Id },
                         Gender = user.Student.Gender,
                         Type = user.Student.Type,
                         Number = user.Student.Number,
                         Education = user.Student.Education,
                         Title = user.Student.Title,
                         Email = user.Student.Email
                     }).ToList();
            return u1;
        }

        public IList<Attendance> ListAttendanceById(long classId, long seminarId)//测试成功
        {
            var u = (from a in _db.Attendences
                     where a.ClassInfo.Id == classId && a.Seminar.Id == seminarId
                     select new Attendance
                     {
                         Id = a.Id,
                         Student = new UserInfo { Id = a.Student.Id },
                         ClassInfo = new ClassInfo { Id = a.ClassInfo.Id },
                         Seminar = new Seminar { Id = a.Seminar.Id },
                         AttendanceStatus = a.AttendanceStatus
                     }).ToList();
            return u;
        }

        public IList<Course> ListCourseByTeacherName(string teacherName)//测试成功
        {
            var u = (from course in _db.Course.Include(a => a.Teacher)
                     where course.Teacher.Name.Equals(teacherName)
                     select new Course
                     {
                         Id = course.Id,
                         Name = course.Name,
                         StartDate = course.StartDate,
                         EndDate = course.EndDate,
                         Teacher = new UserInfo { Id=course.Teacher.Id},
                         Description = course.Description,
                         ReportPercentage = course.ReportPercentage,
                         PresentationPercentage = course.PresentationPercentage,
                         FivePointPercentage = course.FivePointPercentage,
                         FourPointPercentage = course.FourPointPercentage,
                         ThreePointPercentage = course.ThreePointPercentage
                     }).ToList();
            return u;
        }

        public IList<UserInfo> ListLateStudent(long seminarId, long classId)//测试成功
        {
            if (classId <= 0 || seminarId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from class1 in _db.ClassInfo
                     where class1.Id == classId
                     select class1).SingleOrDefault();
            if (u == null)
                throw new ClassNotFoundException();
            var v = (from seminar in _db.Seminar
                     where seminar.Id == seminarId
                     select seminar).SingleOrDefault();
            if (v == null)
                throw new SeminarNotFoundException();
            var u1 = (from user in _db.Attendences//.Include(a => a.Student)user.Student
                      where user.AttendanceStatus == AttendanceStatus.Late && user.Seminar.Id == seminarId && user.ClassInfo.Id == classId
                      select new UserInfo
                      {
                          Id = user.Student.Id,
                          Phone = user.Student.Phone,
                          Avatar = user.Student.Avatar,
                          Password = user.Student.Password,
                          Name = user.Student.Name,
                          School = new School { Id = user.Student.School.Id },
                          Gender = user.Student.Gender,
                          Type = user.Student.Type,
                          Number = user.Student.Number,
                          Education = user.Student.Education,
                          Title = user.Student.Title,
                          Email = user.Student.Email
                      }).ToList();
            return u1;
        }

        public IList<UserInfo> ListPresentStudent(long seminarId, long classId)//测试成功
        {
            if (classId <= 0 || seminarId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from class1 in _db.ClassInfo
                     where class1.Id == classId
                     select class1).SingleOrDefault();
            if (u == null)
                throw new ClassNotFoundException();
            var v = (from seminar in _db.Seminar
                     where seminar.Id == seminarId
                     select seminar).SingleOrDefault();
            if (v == null)
                throw new SeminarNotFoundException();
            var u1 = (from user in _db.Attendences//.Include(a => a.Student)user.Student
                      where user.AttendanceStatus == AttendanceStatus.Present && user.Seminar.Id == seminarId && user.ClassInfo.Id == classId
                      select new UserInfo
                      {
                          Id = user.Student.Id,
                          Phone = user.Student.Phone,
                          Avatar = user.Student.Avatar,
                          Password = user.Student.Password,
                          Name = user.Student.Name,
                          School = new School { Id = user.Student.School.Id },
                          Gender = user.Student.Gender,
                          Type = user.Student.Type,
                          Number = user.Student.Number,
                          Education = user.Student.Education,
                          Title = user.Student.Title,
                          Email = user.Student.Email
                      }).ToList();
            return u1;
        }

        public IList<UserInfo> ListUserByClassId(long classId, string numBeginWith, string nameBeginWith)//测试成功
        {
            var user = (from class1 in _db.CourseSelection
                        from c in _db.UserInfo
                        where class1.ClassId == classId && class1.StudentId == c.Id && c.Name.ToString().StartsWith(nameBeginWith) && c.Number.ToString().StartsWith(numBeginWith)
                        select new UserInfo
                        {
                            Id = class1.StudentId,
                            Phone = class1.Student.Phone,
                            Avatar = class1.Student.Avatar,
                            Password = class1.Student.Password,
                            Name = class1.Student.Name,
                            School = new School { Id = class1.Student.School.Id },
                            Gender = class1.Student.Gender,
                            Type = class1.Student.Type,
                            Number = class1.Student.Number,
                            Education = class1.Student.Education,
                            Title = class1.Student.Title,
                            Email = class1.Student.Email
                        }).ToList();
            return user;
        }

        public IList<UserInfo> ListUserByUserName(string userName)//测试成功
        {
            var user = (from user1 in _db.UserInfo
                        where user1.Name.Equals(userName)
                        select new UserInfo
                        {
                            Id = user1.Id,
                            Phone = user1.Phone,
                            Avatar = user1.Avatar,
                            Password = user1.Password,
                            Name = user1.Name,
                            School = new School { Id = user1.School.Id },
                            Gender = user1.Gender,
                            Type = user1.Type,
                            Number = user1.Number,
                            Education = user1.Education,
                            Title = user1.Title,
                            Email = user1.Email
                        }).ToList();
            return user;
        }

        public IList<long> ListUserIdByUserName(string userName)//测试成功
        {
            var user = (from user1 in _db.UserInfo
                        where user1.Name.Equals(userName)
                        select user1.Id).ToList();
            return user;
        }

        public void UpdateUserByUserId(long userId, UserInfo user)//测试成功
        {
            if (userId <= 0)
            {
                throw new ArgumentException();
            }
            var u = (from user1 in _db.UserInfo
                     where user1.Id == userId
                     select user1).SingleOrDefault();
            if (u == null)
                throw new UserNotFoundException();
            if (user.Name != null) u.Name = user.Name;
            if (user.Number != null) u.Number = user.Number;
            if (user.Password != null) u.Password = user.Password;
            if (user.Email != null) u.Email = user.Email;
            if (user.Title != null) u.Title = user.Title;
            if (user.Type != null) u.Type = user.Type;
            u.School = (from i in _db.School
                        where i.Id == user.School.Id
                        select i).SingleOrDefault();
            if (user.Gender != null) u.Gender = user.Gender;
            if (user.Education != null) u.Education = user.Education;
            _db.SaveChanges();
        }
    }
}
