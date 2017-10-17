using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A Person Entity.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstName">First Name</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="age">Age</param>
        /// <param name="id">Identity for the person</param>
        public Person(string firstName, string lastName, int age, IIdentity<Person> id = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }

        public IIdentity<Person> Id { get; }
        /// <summary>
        /// Gets the first name.
        /// </summary>
        public string FirstName { get; }
        /// <summary>
        /// Gets the last name.
        /// </summary>
        public string LastName { get; }
        /// <summary>
        /// Gets the age.
        /// </summary>
        public int Age { get; }
    }
}
