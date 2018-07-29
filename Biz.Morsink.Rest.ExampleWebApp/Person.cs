using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.ExampleWebApp
{
    /// <summary>
    /// A Person reosurce.
    /// </summary>
    public class Person : IHasIdentity<Person>
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

        IIdentity IHasIdentity.Id => Id;
    }
    public class PersonV2 : IHasIdentity<PersonV2>
    {
        public static PersonV2 Create(Person person)
            => new PersonV2(person.FirstName, person.LastName, DateTime.Now.Date.AddYears(-person.Age), person.Id == null ? null : FreeIdentity<PersonV2>.Create(person.Id.Value));
        public Person ToV1()
            => new Person(FirstName, LastName,
                DateTime.Now.Year - Birthday.Year - (DateTime.Now.Month > Birthday.Month || DateTime.Now.Month == Birthday.Month && DateTime.Now.Day >= Birthday.Day ? 0 : 1),
                Id.Value == null ? null : FreeIdentity<Person>.Create(Id.Value));
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstName">First Name</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="birthday">Birthday</param>
        /// <param name="id">Identity for the person</param>
        public PersonV2(string firstName, string lastName, DateTime birthday, IIdentity<PersonV2> id = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Birthday = birthday;
        }

        public IIdentity<PersonV2> Id { get; }
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
        public DateTime Birthday { get; }

        IIdentity IHasIdentity.Id => Id;

    }
}
