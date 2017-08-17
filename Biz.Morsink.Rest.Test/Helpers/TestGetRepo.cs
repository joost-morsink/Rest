﻿using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public class TestGetRepo : RestRepository<Person>, IRestGet<Person, NoParameters>
    {
        public ValueTask<RestResult<Person>> Get(IIdentity<Person> id, NoParameters np)
        {
            if (id.Value?.ToString() == "1")
            {
                var p = new Person { FirstName = "Joost", LastName = "Morsink", Age = 37 };
                return Rest.Value(p).ToResultAsync();
            }
            else
                return RestResult.NotFound<Person>().ToAsync();
        }
    }
    public class TestGetFriendCollectionRepo : RestRepository<PersonFriendCollection>, IRestGet<PersonFriendCollection, NoParameters>
    {
        public ValueTask<RestResult<PersonFriendCollection>> Get(IIdentity<PersonFriendCollection> id, NoParameters parameters)
        {
            if (id.Value?.ToString() == "1")
            {
                // I am my own best friend :)
                var pfc = new PersonFriendCollection { PersonId = FreeIdentity<Person>.Create(1), FriendIds = new IIdentity<Person>[] { FreeIdentity<Person>.Create(1) } };
                return Rest.Value(pfc).ToResultAsync();
            }
            else
                return RestResult.NotFound<PersonFriendCollection>().ToAsync();
        }
    }
    public class PersonFriendCollectionLinkProvider : ILinkProvider<Person>
    {
        public IReadOnlyList<Link> GetLinks(IIdentity<Person> id)
            => new[] 
            {
                Link.Create("friends", FreeIdentity<PersonFriendCollection>.Create(id.Value))
            };
    }
    public class AgeFactorParameter
    {
        public AgeFactorParameter(double ageFactor)
        {
            AgeFactor = ageFactor;
        }
        public double AgeFactor { get; }
    }
    public class TestGetRepo2 : RestRepository<Person2>, IRestGet<Person2, AgeFactorParameter>, IRestGet<Person2, NoParameters>
    {
        public ValueTask<RestResult<Person2>> Get(IIdentity<Person2> id, AgeFactorParameter parameters)
        {
            if (id.Value?.ToString() == "1")
            {
                var p = new Person2 { FirstName = "Joost", LastName = "Morsink", Age = (int)(37 * parameters.AgeFactor) };
                return Rest.Value(p).ToResultAsync();
            }
            else
                return RestResult.NotFound<Person2>().ToAsync();
        }

        public ValueTask<RestResult<Person2>> Get(IIdentity<Person2> id, NoParameters parameters)
        {
            if (id.Value?.ToString() == "1")
            {
                var p = new Person2 { FirstName = "Joost", LastName = "Morsink", Age = 37 };
                return Rest.Value(p).ToResultAsync();
            }
            else
                return RestResult.NotFound<Person2>().ToAsync();
        }
    }
}
