using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApplicationCode
{
    public class Student
    {
        public string id;
        public string fullName;
        public string emailAddress;

        public Student(string id, string fullName, string emailAddress)
        {
            this.id = id;
            this.fullName = fullName;
            this.emailAddress = emailAddress;
        }


    }
}