using MongoDB.Driver;
using AutodeskViewerAPI.Models;
using Microsoft.Extensions.Options;
using AutodeskViewerAPI.Settings;

namespace AutodeskViewerAPI.Services
{
    public class CommentsMongoDBService
    {
        private readonly IMongoCollection<ModelPartInfo> _modelPartInfoCollection;

        public CommentsMongoDBService(
            IOptions<MongoDBSettings> mongoDBSettings,
            IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _modelPartInfoCollection = database.GetCollection<ModelPartInfo>("ModelPartInfos");
        }

        public async Task<List<ModelPartInfo>> GetAllModelsAsync() =>
            await _modelPartInfoCollection.Find(_ => true).ToListAsync();

        public async Task<ModelPartInfo?> GetModelAsync(string modelURN) =>
            await _modelPartInfoCollection.Find(x => x.modelURN == modelURN).FirstOrDefaultAsync();

        //Find and return specific part from specific model
        //<ModelPart?> means it can return null which we want in here if not found
        public async Task<ModelPart?> GetPartAsync(string modelURN, int partID)
        {
            //Filter to find the document with the specified modelURN
            var filter = Builders<ModelPartInfo>.Filter.Eq(m => m.modelURN, modelURN) &
                         Builders<ModelPartInfo>.Filter.ElemMatch(m => m.Parts, p => p.PartId == partID);

            //Projection to return only the matched part
            var projection = Builders<ModelPartInfo>.Projection.Expression(m => m.Parts.FirstOrDefault(p => p.PartId == partID));

            var result = await _modelPartInfoCollection.Find(filter)
                                                       .Project(projection)
                                                       .FirstOrDefaultAsync();

            return result;
        }

        //Get comments of specific part in specific URN
        public async Task<Dictionary<string, string>?> GetCommentsAsync(string modelURN, int partID)
        {
            //Filter to find the document with the specified modelURN
            var filter = Builders<ModelPartInfo>.Filter.Eq(m => m.modelURN, modelURN) &
                         Builders<ModelPartInfo>.Filter.ElemMatch(m => m.Parts, p => p.PartId == partID);

            //Projection to return only the matched part
            var projection = Builders<ModelPartInfo>.Projection.Expression(
                m => m.Parts.FirstOrDefault(p => p.PartId == partID).comments
            );

            var result = await _modelPartInfoCollection.Find(filter)
                                                       .Project(projection)
                                                       .FirstOrDefaultAsync();

            return result;
        }


        public async Task<bool> AddCommentAsync(string modelURN, int partID, string commentKey, string commentValue)
        {
            //Filter to find the document with the specific modelURN and PartId
            var filter = Builders<ModelPartInfo>.Filter.And(
                Builders<ModelPartInfo>.Filter.Eq(m => m.modelURN, modelURN),
                Builders<ModelPartInfo>.Filter.Eq("Parts.PartId", partID) // Match the specific PartId in the array
            );

            //Update to add or update the comment in the comments dictionary of the matched part using the positional operator $
            var update = Builders<ModelPartInfo>.Update.Set(
                "Parts.$.comments." + commentKey, commentValue
            );

            //Execute the update
            var updateResult = await _modelPartInfoCollection.UpdateOneAsync(filter, update);

            //Return true if a document was modified (comment was added)
            return updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCommentAsync(string modelURN, int partID, string commentKey)
        {
            //Filter to find the document with the specific modelURN and PartId
            var filter = Builders<ModelPartInfo>.Filter.And(
                Builders<ModelPartInfo>.Filter.Eq(m => m.modelURN, modelURN),
                Builders<ModelPartInfo>.Filter.Eq("Parts.PartId", partID) // Match the specific PartId in the array
            );

            //Update to remove the comment with the specified key from the comments dictionary
            var update = Builders<ModelPartInfo>.Update.Unset("Parts.$.comments." + commentKey);

            //Execute the update
            var updateResult = await _modelPartInfoCollection.UpdateOneAsync(filter, update);

            //Return true if a document was modified (comment was removed)
            return updateResult.ModifiedCount > 0;
        }


        public async Task<bool> AddPartAsync(string modelURN, ModelPart newPartId)
        {
            //Filter to find the document with the specified modelURN
            var filter = Builders<ModelPartInfo>.Filter.Eq(m => m.modelURN, modelURN);

            //Update definition to add the new part to the Parts array
            var update = Builders<ModelPartInfo>.Update.AddToSet(m => m.Parts, newPartId);

            //Execute the update
            var result = await _modelPartInfoCollection.UpdateOneAsync(filter, update);

            //Return true if a document was modified
            return result.ModifiedCount > 0;
        }

        public async Task CreateAsync(ModelPartInfo newModelPartInfo) =>
            await _modelPartInfoCollection.InsertOneAsync(newModelPartInfo);

        public async Task UpdateAsync(string modelURN, ModelPartInfo updatedModelPartInfo) =>
            await _modelPartInfoCollection.ReplaceOneAsync(x => x.modelURN == modelURN, updatedModelPartInfo);

        public async Task RemoveAsync(string modelURN) =>
            await _modelPartInfoCollection.DeleteOneAsync(x => x.modelURN == modelURN);


    }
}
