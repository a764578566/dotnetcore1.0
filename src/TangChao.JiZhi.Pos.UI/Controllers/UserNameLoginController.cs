using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TangChao.JiZhi.Pos.Models;
using TangChao.JiZhi.Pos.Bizlogic;
using TangChao.JiZhi.Pos.IBizlogic;
using TangChao.JiZhi.Pos.ViewModels;
using Microsoft.Extensions.Logging;
using TangChao.JiZhi.Pos.Common;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TangChao.JiZhi.Pos.UI.Controllers
{
    [Route("api/[controller]")]
    public class UserNameLoginController : Controller
    {
        private IUserNameBLL userNameBll ;
        private readonly ILogger<UserNameLoginController> _logger;

        public UserNameLoginController(IUserNameBLL userNameBll, ILogger<UserNameLoginController> logger)
        {
            this.userNameBll = userNameBll;
            _logger = logger;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation(LoggingEvents.LIST_ITEMS, "Listing all items");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public UserNameLoginVM Get(int id)
        {
            return userNameBll.GetUserName(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {

        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
