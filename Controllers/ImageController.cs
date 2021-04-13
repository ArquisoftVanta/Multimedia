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

                var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

                if (multimedia != null)
                {

                    var multimediaPath = multimedia.location;
                    if(System.IO.File.Exists(multimediaPath)){

                        var fileBytes = System.IO.File.ReadAllBytes(multimediaPath);

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