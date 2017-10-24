﻿using Biz.Morsink.Rest.ExampleWebApp;
using Biz.Morsink.Rest.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.AspNetCore.Test
{
    [TestClass]
    public class ExampleAppTest
    {
        public const string SERVER = "localhost";
        public const int PORT = 5000;
        public const string SCHEME = "http";
        public static readonly string URL = $"{SCHEME}://{SERVER}:{PORT}";

        private const string properties = nameof(properties);
        private const string id = nameof(id);
        private const string firstName = nameof(firstName);
        private const string lastName = nameof(lastName);
        private const string age = nameof(age);
        private const string describedby = nameof(describedby);
        private const string admin = nameof(admin);
        private const string dollarRef = "$ref";
        private const string count = nameof(count);
        private const string items = nameof(items);
        private const string limit = nameof(limit);
        private const string skip = nameof(skip);

        private const string GET = nameof(GET);
        private const string OPTIONS = nameof(OPTIONS);

        private static string SchemaFor<T>()
            => "/schema/" + typeof(T).ToString().Replace(".", "%2E");


        private static ImmutableDictionary<string, string> DefaultHeaders = ImmutableDictionary<string, string>.Empty.Add("Accept", "application/json");
        private static CancellationTokenSource cts;
        private HttpClient client;
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            cts = new CancellationTokenSource();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.RunAsync(cts.Token);

            var connected = false;
            while (!connected)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
                {
                    try
                    {
                        socket.Connect(SERVER, PORT);
                        connected = socket.Connected;
                    }
                    catch
                    {
                        connected = false;
                        Thread.Sleep(500);
                    }
                }
            }
        }
        [TestInitialize]
        public void Init()
        {
            client = new HttpClient();
        }
        [ClassCleanup]
        public static void Exit()
        {
            cts.Cancel();
        }
        [TestCleanup]
        public void TestExit()
        {
            client.Dispose();
        }
        private Task<HttpResponseMessage> Get(HttpClient client, string path, IReadOnlyDictionary<string, string> headers = null)
        {
            headers = headers ?? DefaultHeaders;
            foreach (var kvp in headers)
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            return client.GetAsync(URL + path);
        }
        private Task<HttpResponseMessage> Options(HttpClient client, string path, IReadOnlyDictionary<string, string> headers = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Options, URL + path);
            headers = headers ?? DefaultHeaders;
            foreach (var kvp in headers)
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            return client.SendAsync(req);
        }
        private class Link
        {
            public string Address { get; set; }
            public string Reltype { get; set; }
        }
        private Link ParseLink(string link)
        {
            var parts = link.Split(";");
            return parts.Length == 2 && parts[0].StartsWith("<") && parts[0].EndsWith(">") && parts[1].StartsWith("rel=")
                ? new Link { Address = parts[0].Substring(1, parts[0].Length - 2), Reltype = parts[1].Substring(4) }
                : null;
        }
        private Dictionary<string, string> ParseLinks(IEnumerable<string> links)
        {
            return links.Select(l => ParseLink(l)).ToDictionary(x => x.Reltype, x => x.Address);
        }
        [TestMethod]
        public async Task Http_CheckHome()
        {
            var resp = await Get(client, "/");
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.TryGetValues("Link", out var linkStrings));
            var links = linkStrings.Select(ParseLink).Where(l => l != null);

            var desc = links.FirstOrDefault(l => l.Reltype == describedby);
            var adm = links.FirstOrDefault(l => l.Reltype == admin);

            Assert.IsNotNull(desc);
            Assert.IsNotNull(adm);

            Assert.AreEqual(SchemaFor<Home>(), desc.Address);
            Assert.AreEqual("/person/1", adm.Address);
        }
        [TestMethod]
        public async Task Http_CheckPerson()
        {
            var resp = await Get(client, "/person/1");
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.TryGetValues("Link", out var linkStrings));
            var links = linkStrings.Select(ParseLink).Where(l => l != null);
            var desc = links.FirstOrDefault(l => l.Reltype == describedby);

            Assert.IsNotNull(desc);
            Assert.AreEqual(SchemaFor<ExampleWebApp.Person>(), desc.Address);
        }
        [TestMethod]
        public async Task Http_CheckPersonCollection()
        {

            var resp = await Get(client, "/person?q=Joost&limit=10&skip=0");
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var links = ParseLinks(resp.Headers.GetValues("Link"));
            Assert.IsTrue(links.ContainsKey("first"));
            Assert.IsTrue(links.ContainsKey("last"));

            var body = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);
            Assert.IsNotNull(json[id]);
            Assert.IsNotNull(json[count]);
            Assert.AreEqual(1, json[count].Value<int>());
            Assert.IsNotNull(json[limit]);
            Assert.AreEqual(10, json[limit].Value<int>());
            Assert.IsNotNull(json[skip]);
            Assert.AreEqual(0, json[skip].Value<int>());
            Assert.IsNotNull(json[items]);
            Assert.AreEqual(1, (json[items] as JArray)?.Count);
        }
        [TestMethod]
        public async Task Http_CheckPersonCollectionEmpty()
        {
            var resp = await Get(client, "/person?q=x&limit=10&skip=0");
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var body = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);
            Assert.IsNotNull(json[id]);
            Assert.IsNotNull(json[count]);
            Assert.AreEqual(0, json[count].Value<int>());
            Assert.IsNotNull(json[items]);
            Assert.AreEqual(0, (json[items] as JArray)?.Count);
        }
        [TestMethod]
        public async Task Http_CheckSchema()
        {
            var resp = await Get(client, SchemaFor<ExampleWebApp.Person>());
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.TryGetValues("Link", out var linkStrings));
            var links = linkStrings.Select(ParseLink).Where(l => l != null);
            var desc = links.FirstOrDefault(l => l.Reltype == describedby);

            Assert.IsNotNull(desc);
            Assert.AreEqual(SchemaFor<TypeDescriptor>(), desc.Address);

            var body = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);
            Assert.IsNotNull(json[properties]);
            Assert.IsNotNull(json[properties][id]);
            Assert.IsNotNull(json[properties][firstName]);
            Assert.IsNotNull(json[properties][lastName]);
            Assert.IsNotNull(json[properties][age]);
        }
        [TestMethod]
        public async Task Http_CheckMetaSchema()
        {
            var resp = await Get(client, SchemaFor<TypeDescriptor>());
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.TryGetValues("Link", out var linkStrings));
            var links = linkStrings.Select(ParseLink).Where(l => l != null);
            var desc = links.FirstOrDefault(l => l.Reltype == describedby);

            Assert.IsNotNull(desc);
            Assert.AreEqual(SchemaFor<TypeDescriptor>(), desc.Address);

            var body = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);

            Assert.IsNotNull(json[dollarRef]);
            Assert.AreEqual("http://json-schema.org/draft-04/schema#", json[dollarRef].Value<string>());
        }

        [TestMethod]
        public async Task Http_CheckOptions()
        {
            var resp = await Options(client, "/");
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var allowStrings = resp.Content.Headers.Allow;
            Assert.IsTrue(allowStrings.Count > 0);
            Assert.IsTrue(allowStrings.Contains(GET));
            Assert.IsTrue(allowStrings.Contains(OPTIONS));

            await Get(client, "/");
            Assert.IsTrue(resp.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Http_InvalidAddress()
        {
            var resp = await Get(client, "/Invalid/Address");
            Assert.IsFalse(resp.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }
    }
}
