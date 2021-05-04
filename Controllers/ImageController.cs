using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using multimedia_storage.Context;
using multimedia_storage.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace multimedia_storage.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {

        public readonly AppDbContext context;
        private readonly IWebHostEnvironment _environment;

        public ImageController(AppDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            _environment = env;
        }

        // GET <ImagesController>/5
        /// <summary>
        /// Get a image file
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A existly image file</returns>
        /// <response code="200">Returns the existly image file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the image file is null</response>
        [HttpGet("{id}", Name = "GetImage")]
        public ActionResult Get(int id)
        {
            try
            {

                if(id <= 0){
                    return BadRequest();
                }

                var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

                if (multimedia != null)
                {

                    var multimediaPath = Path.Combine(_environment.ContentRootPath, "storage",multimedia.name + "." + multimedia.extension);
                    if(System.IO.File.Exists(multimediaPath)){

                        var fileBytes = Convert.FromBase64String(multimedia.image);
                        var fileMemStream = new MemoryStream(fileBytes);

                        return Ok(fileMemStream);
                            
                    }

                    return NotFound();
                    
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
