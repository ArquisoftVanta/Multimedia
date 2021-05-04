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
    public class MultimediaController : ControllerBase
    {
        public readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public MultimediaController(AppDbContext _context, IWebHostEnvironment env)
        {
            this._context = _context;
            _environment = env;
        }

        // GET: <MultimediasController>
        /// <summary>
        /// Gets all multimedia files 
        /// </summary>
        /// <returns>All existly multimedia files</returns>
        /// <response code="200">Returns all existly multimedia files</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia files is null</response>
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                return Ok(_context.multimedias.ToList());

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET <MultimediasController>/5
        /// <summary>
        /// Get a multimedia file
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A existly multimedia file</returns>
        /// <response code="200">Returns the existly multimedia file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response>
        [HttpGet("{id}", Name = "GetMultimedia")]
        public ActionResult Get(int id)
        {
            try
            {

                if (id <= 0)
                {
                    return BadRequest();
                }

                var multimedia = _context.multimedias.FirstOrDefault(m => m.id == id);

                if (multimedia != null)
                {
                    return Ok(multimedia);
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

        // POST <MultimediasController>
        /// <summary>
        /// Create a new multimedia file
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// POST /Todo
        ///     {
        ///        "file": "select_file"
        ///     }
        /// </remarks>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <response code="200">Returns the newly created multimedia file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response>
        [HttpPost]
        public ActionResult Post([FromForm] IFormFile file)
        {

            try
            {

                string base64Text;

                if (file != null)
                {

                    var storageFolder = Path.Combine(_environment.ContentRootPath, "storage");
                    Directory.CreateDirectory(storageFolder);

                    var filePath = Path.Combine(_environment.ContentRootPath, "storage", file.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                        fileStream.Close();
                        fileStream.Dispose();
                    }

                    using (var fileMemStream = new MemoryStream())
                    {
                        file.CopyTo(fileMemStream);
                        var fileBytes = fileMemStream.ToArray();
                        base64Text = Convert.ToBase64String(fileBytes);
                        fileMemStream.Close();
                        fileMemStream.Dispose();
                    }

                    double size = file.Length;
                    size = Math.Round(size, 2);

                    Multimedia multimedia = new Multimedia();
                    multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                    multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                    multimedia.size = size;
                    multimedia.location = filePath;
                    multimedia.image = base64Text;

                    _context.multimedias.Add(multimedia);
                    _context.SaveChanges();
                    return CreatedAtRoute("GetMultimedia", new { id = multimedia.id }, multimedia);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT <MultimediasController>/5
        /// <summary>
        /// Modifies an existing multimedia file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns>A modified multimedia file</returns>
        /// <response code="200">Returns the modified multimedia file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response>
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromForm] IFormFile file)
        {

            try
            {

                string base64Text;

                if (file != null)
                {

                    if (id <= 0)
                    {
                        return BadRequest();
                    }

                    var multimedia = _context.multimedias.FirstOrDefault(m => m.id == id);

                    if (multimedia != null)
                    {

                        var multimediaPath = Path.Combine(_environment.ContentRootPath, "storage", multimedia.name + "." + multimedia.extension);

                        if (System.IO.File.Exists(multimediaPath))
                        {
                            System.IO.File.Delete(multimediaPath);
                        }

                        var storageFolder = Path.Combine(_environment.ContentRootPath, "storage");
                        Directory.CreateDirectory(storageFolder);

                        var filePath = Path.Combine(_environment.ContentRootPath, "storage", file.FileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Close();
                            fileStream.Dispose();
                        }

                        using (var fileMemStream = new MemoryStream())
                        {
                            file.CopyTo(fileMemStream);
                            var fileBytes = fileMemStream.ToArray();
                            base64Text = Convert.ToBase64String(fileBytes);
                            fileMemStream.Close();
                            fileMemStream.Dispose();
                        }

                        double size = file.Length;
                        size = size / 1000000;
                        size = Math.Round(size, 2);
                        multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                        multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                        multimedia.size = size;
                        multimedia.location = filePath;
                        multimedia.image = base64Text;

                        _context.Entry(multimedia).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _context.SaveChanges();
                        return CreatedAtRoute("GetMultimedia", new { id = multimedia.id }, multimedia);
                    }
                    else
                    {
                        return NotFound();
                    }

                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // DELETE <MultimediasController>/5
        /// <summary>
        /// Removes a multimedia file
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A multimedia file</returns>
        /// <response code="200">Returns multimedia file id</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response> 
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {

            try
            {

                if (id <= 0)
                {
                    return BadRequest();
                }

                var multimedia = _context.multimedias.FirstOrDefault(m => m.id == id);

                if (multimedia != null)
                {

                    var multimediaPath = Path.Combine(_environment.ContentRootPath, "storage", multimedia.name + "." + multimedia.extension);

                    if (System.IO.File.Exists(multimediaPath))
                    {
                        System.IO.File.Delete(multimediaPath);
                    }

                    _context.multimedias.Remove(multimedia);
                    _context.SaveChanges();
                    return Ok(multimedia);
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
