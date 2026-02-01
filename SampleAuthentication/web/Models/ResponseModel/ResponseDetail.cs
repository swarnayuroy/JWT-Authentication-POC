using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace web.Models.ResponseModel
{
    public class ResponseDetail
    {
        public bool Status { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public string Message { get; set; }
    }
}