using Microsoft.AspNetCore.Mvc;
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

        public readonly AppDbContext context;

        public MultimediaController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: <MultimediasController>
        /// <summary>
        /// Gets all multimedia files 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                return Ok(context.multimedias.ToList());

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
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetMultimedia")]
        public ActionResult Get(int id)
        {
            try
            {

                var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

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
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Post([FromForm] IFormFile file)
        {
            try
            {

                if (file != null)
                {

                    var filePath = "C:\\Users\\Leron\\Documents\\source\\vanta_multimedia_storage\\storage\\" + file.FileName;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    double size = file.Length;
                    size = size / 1000000;
                    size = Math.Round(size, 2);
                    Multimedia multimedia = new Multimedia();
                    multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                    multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                    multimedia.size = size;
                    multimedia.location = filePath;

                    context.multimedias.Add(multimedia);
                    context.SaveChanges();
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
        /// <returns></returns>
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromForm] IFormFile file)
        {

            try
            {

                if (file != null)
                {

                    var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

                    if (multimedia != null)
                    {

                        var filePath = "C:\\Users\\Leron\\Documents\\source\\vanta_multimedia_storage\\storage\\" + file.FileName;
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        double size = file.Length;
                        size = size / 1000000;
                        size = Math.Round(size, 2);
                        multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                        multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                        multimedia.size = size;
                        multimedia.location = filePath;

                        context.Entry(multimedia).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        context.SaveChanges();
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
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {

            try
            {

                var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

                if (multimedia != null)
                {
                    context.multimedias.Remove(multimedia);
                    context.SaveChanges();
                    return Ok(id);
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
