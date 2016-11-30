using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TangChao.JiZhi.Pos.IBizlogic;
using Microsoft.Extensions.Logging;
using TangChao.JiZhi.Pos.Common;
using TangChao.JiZhi.Pos.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TangChao.JiZhi.Pos.UI.Controllers
{
    [Route("api/[controller]")]
    public class SysProjectController : Controller
    {
        private ISysProjectBLL sysProjectBll;
        private readonly ILogger<SysProjectController> _logger;

        public SysProjectController(ISysProjectBLL sysProjectBll, ILogger<SysProjectController> logger)
        {
            this.sysProjectBll = sysProjectBll;
            _logger = logger;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<SysProjectVM> Get()
        {
            return new SysProjectVM[] { sysProjectBll.GetProject(2) };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public SysProjectVM Get(int id)
        {
            return sysProjectBll.GetProject(id);
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
