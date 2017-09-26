using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    public class Person
    {
        public Person(/*IIdentity<Person> id,*/ string firstName, string lastName, int age)
        {
            //Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }

        //public IIdentity<Person> Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public int Age { get; }
    }
}
