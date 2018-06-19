﻿using Biz.Morsink.Rest.AspNetCore;
using Biz.Morsink.Rest.AspNetCore.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.HttpConverter.HalJson
{
    public class HalJsonHttpConverter : AbstractHttpRestConverter
    {
        public const string MEDIA_TYPE = "application/hal+json";
        private readonly IOptions<HalJsonConverterOptions> options;
        public HalJsonHttpConverter(IOptions<HalJsonConverterOptions> options, IRestIdentityProvider provider, IOptions<RestAspNetCoreOptions> restOptions)
            : base(provider, restOptions)
        {
            this.options = options;
        }
        public override decimal AppliesScore(HttpContext context)
            => ScoreAcceptHeader(context.Request, MEDIA_TYPE);
        public override object ParseBody(Type t, byte[] body)
        {
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var jtr = new JsonTextReader(sr))
            {
                var ser = JsonSerializer.Create(options.Value.SerializerSettings);
                try
                {
                    return ser.Deserialize(jtr, t);
                }
                catch (Exception e)
                {
                    throw new RestFailureException(RestResult.BadRequest<object>($"Parse error: {e.Message}"), e.Message, e);
                }
            }
        }
        protected override void ApplyGeneralHeaders(HttpResponse httpResponse, RestResponse response)
        {
            httpResponse.ContentType = MEDIA_TYPE;
        }
        protected override void ApplyHeaders(HttpResponse httpResponse, RestResponse response, IRestValue value, RestPrefixContainer prefixes)
        {
        }

        protected override async Task WriteValue(Stream bodyStream, RestResponse response, IRestResult result, IRestValue value)
        {
            var ser = JsonSerializer.Create(options.Value.SerializerSettings);

            using (var ms = new MemoryStream())
            {
                using (var swri = new StreamWriter(ms))
                using (var wri = new JsonTextWriter(swri))
                    ser.Serialize(wri, value.Value);

                var body = ms.ToArray();
                await bodyStream.WriteAsync(body, 0, body.Length);
            }
        }
        public override bool SupportsCuries => false;
    }
}
