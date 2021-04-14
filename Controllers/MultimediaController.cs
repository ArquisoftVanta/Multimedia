using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using multimedia_storage.Context;
using multimedia_storage.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Firebase.Storage;
using Firebase.Auth;
using System.Threading.Tasks;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace multimedia_storage.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MultimediaController : ControllerBase
    {

        public readonly AppDbContext context;
        private readonly IWebHostEnvironment _environment;

        // Configure Firebase
        private static string apiKey = "AIzaSyCCQVALEPSDGxNh22RB3aCVvq0NtMtJxtc";
        private static string Bucket = "vanta-multimedia.appspot.com";
        private static string AuthEmail = "sebasduca@gmail.com";
        private static string AuthPassword = "lerona";

        public MultimediaController(AppDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
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
        /// <returns>A existly multimedia file</returns>
        /// <response code="200">Returns the existly multimedia file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response>
        [HttpGet("{id}", Name = "GetMultimedia")]
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
        public async Task<IActionResult> Index([FromForm] IFormFile file)
        {
            try
            {
                
                FileStream fileStream;

                if (file != null)
                {

                    var storageFolder = Path.Combine(_environment.ContentRootPath, "storage");
                    Directory.CreateDirectory(storageFolder);

                    var filePath = Path.Combine(_environment.ContentRootPath,"storage",file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    fileStream = new FileStream(filePath, FileMode.Open);

                    double size = file.Length;
                    size = size / 1000000;
                    size = Math.Round(size, 2);
                    Multimedia multimedia = new Multimedia();
                    multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                    multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                    multimedia.size = size;

                    // Firebase authentication
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
                    var token = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                    var cancellation = new CancellationTokenSource();

                    var uploadFirebase = new FirebaseStorage(Bucket, new FirebaseStorageOptions{
                        AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("storage")
                    .Child($"{multimedia.name}.{multimedia.extension}")
                    .PutAsync(fileStream, cancellation.Token);

                    var fileUrl = "";

                    try{

                        fileUrl = await uploadFirebase;

                    }catch(Exception ex){
                        return BadRequest(ex.Message);
                    }

                    multimedia.location = fileUrl;

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
        /// <returns>A modified multimedia file</returns>
        /// <response code="200">Returns the modified multimedia file</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Index(int id, [FromForm] IFormFile file)
        {

            try
            {

                FileStream fileStream;

                if (file != null)
                {

                    if(id <= 0){
                        return BadRequest();
                    }

                    var multimedia = context.multimedias.FirstOrDefault(m => m.id == id);

                    if (multimedia != null)
                    {

                        var multimediaPath = Path.Combine(_environment.ContentRootPath, "storage",multimedia.name + "." + multimedia.extension);
                        
                        if(System.IO.File.Exists(multimediaPath)){
                            System.IO.File.Delete(multimediaPath);
                        }

                        var storageFolder = Path.Combine(_environment.ContentRootPath, "storage");
                        Directory.CreateDirectory(storageFolder);

                        var filePath = Path.Combine(_environment.ContentRootPath,"storage",file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        fileStream = new FileStream(filePath, FileMode.Open);

                        double size = file.Length;
                        size = size / 1000000;
                        size = Math.Round(size, 2);
                        multimedia.name = Path.GetFileNameWithoutExtension(file.FileName);
                        multimedia.extension = Path.GetExtension(file.FileName).Substring(1);
                        multimedia.size = size;
                        
                        // Firebase authentication
                        var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
                        var token = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                        var cancellation = new CancellationTokenSource();

                        var uploadFirebase = new FirebaseStorage(Bucket, new FirebaseStorageOptions{
                            AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child("storage")
                        .Child($"{multimedia.name}.{multimedia.extension}")
                        .PutAsync(fileStream, cancellation.Token);

                        var fileUrl = "";

                        try{

                            fileUrl = await uploadFirebase;

                        }catch(Exception ex){
                            return BadRequest(ex.Message);
                        }

                        multimedia.location = fileUrl;

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
        /// <returns>A multimedia file</returns>
        /// <response code="200">Returns multimedia file id</response>
        /// <response code="400">If the request is bad structured</response>
        /// <response code="404">If the multimedia file is null</response> 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Index(int id)
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
                        System.IO.File.Delete(multimediaPath);
                    }

                    // Firebase authentication
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
                    var token = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                    var storage = new FirebaseStorage(Bucket);

                    var cancellation = new CancellationTokenSource();

                    var firebaseStorage = new FirebaseStorage(Bucket, new FirebaseStorageOptions{
                        AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("storage")
                    .Child($"{multimedia.name}.{multimedia.extension}")
                    .DeleteAsync();

                    context.multimedias.Remove(multimedia);
                    context.SaveChanges();
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
