using AutodeskViewerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using AutodeskViewerAPI.Services;

namespace AutodeskViewerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : ControllerBase
    {
        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        //private static List<ModelPartInfo> models = new List<ModelPartInfo>();

        private readonly CommentsMongoDBService _commentsMongoDBService;

        public ModelController( CommentsMongoDBService commentsMongoDBService) {
            _commentsMongoDBService = commentsMongoDBService;
        }

        // Create a new model with Id and Name
        //Uses semaphores to avoid race conditions while adding to models with concurrent multiple requests
        [HttpPost("create")]
        public async Task<IActionResult> CreateModel([FromBody] ModelPartInfoDTO newModel)
        {
            if (newModel == null)
            {
                return BadRequest("Model data is required.");
            }

            var model = await _commentsMongoDBService.GetModelAsync(newModel.modelURN);

            if(model != null)
            {
                return BadRequest("Model with this URN already exists!");
            }

            var temp1 = new ModelPartInfo
            {
                modelURN = newModel.modelURN
            };

            
            await _commentsMongoDBService.CreateAsync(temp1);

            return Ok(temp1);
        }


        [HttpGet("GetAllModels")]
        public async Task<IActionResult> getAllModels()
        {
            var tempModels = await _commentsMongoDBService.GetAllModelsAsync();

            return Ok(tempModels);
        }

        [HttpGet("{modelURN}")]
        public async Task<IActionResult> getFullModelInfo(string modelURN)
        {
            var model = await _commentsMongoDBService.GetModelAsync(modelURN);
            if (model == null)
            {
                return NotFound("Model not found.");
            }
            return Ok(model);
        }

        // Add a new part to a model
        [HttpPost("{modelURN}/addPart")]
        public async Task<IActionResult> AddPartToModel(string modelURN, [FromBody] addPartDTO newPart)
        {
            var model = await _commentsMongoDBService.GetModelAsync(modelURN);
            if (model == null)
            {
                return NotFound("Model not found.");
            }

            var part = await _commentsMongoDBService.GetPartAsync(modelURN, newPart.partId);
            if ( part != null)
            {
                return BadRequest("Part with this ID already exists in the model.");
            }

            ModelPart temp1 = new ModelPart()
            {
                PartId = newPart.partId
            };
            try 
            {
                await _commentsMongoDBService.AddPartAsync(modelURN, temp1);
                return Ok(model);
            }
            catch
            {
                return NoContent();
            }
        }

        //Add Comment
        [HttpPost("{modelURN}/part/{partId}/add")]
        public async Task<IActionResult> AddStringToPart(string modelURN, int partId, [FromBody] addCommentDTO newComment)
        {
            if( newComment.key == null || newComment.value == null)
            {
                return NotFound("Comment not found.");
            }

            var model = await _commentsMongoDBService.GetModelAsync(modelURN);
            if (model == null)
            {
                return NotFound("Model not found.");
            }

            var part = await _commentsMongoDBService.GetPartAsync(modelURN, partId);
            if (part == null)
            {
                return NotFound("Model part not found.");
            }

            var res = await _commentsMongoDBService.AddCommentAsync(modelURN, partId, newComment.key, newComment.value);
            
            if(res)
            {
                return Ok();
            }
            else
            {
                return NoContent();
            }
        }


        [HttpGet("{modelURN}/part/{partId}")]
        public async Task<IActionResult> getCommentsOfPart(string modelURN, int partId)
        {
            var model = await _commentsMongoDBService.GetModelAsync(modelURN);
            if (model == null)
            {
                return NotFound("Model not found.");
            }

            var part = await _commentsMongoDBService.GetPartAsync(modelURN, partId);
            if (part == null)
            {
                return NotFound("Model part not found.");
            }

            var temp = await _commentsMongoDBService.GetCommentsAsync(modelURN, partId);

            return Ok(temp);
        }


        // Remove a string from a part's HashMap
        [HttpPost("{modelURN}/part/{partId}/remove/{commentId}")]
        public async Task<IActionResult> RemoveStringFromPart(string modelURN, int partId, string commentId)
        {
            var model = await _commentsMongoDBService.GetModelAsync(modelURN);
            if (model == null)
            {
                return NotFound("Model not found.");
            }

            var part = await _commentsMongoDBService.GetPartAsync(modelURN, partId);
            if (part == null)
            {
                return NotFound("Model part not found.");
            }

            var isDeleted = await _commentsMongoDBService.DeleteCommentAsync(modelURN, partId, commentId);
            
            if(isDeleted)
            {
                return Ok(part);
            }
            else
            {
                return NoContent();
            }


        }

        //// Delete a part from a model
        //[HttpDelete("{id}/part/{partId}")]
        //public IActionResult DeletePartFromModel(int id, int partId)
        //{
        //    var model = models.FirstOrDefault(m => m.Id == id);
        //    if (model == null)
        //    {
        //        return NotFound("Model not found.");
        //    }

        //    var part = model.Parts.FirstOrDefault(p => p.PartId == partId);
        //    if (part == null)
        //    {
        //        return NotFound("Model part not found.");
        //    }

        //    model.Parts.Remove(part);
        //    return Ok(model);
        //}
    }
}
