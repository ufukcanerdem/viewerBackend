using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AutodeskViewerAPI.Models
{
    public class ModelPart
    {
        public int PartId { get; set; }
        public Dictionary<string, string> comments { get; set; }

        public ModelPart()
        {
            comments = new Dictionary<string, string>();
        }
    }

    public class ModelPartInfo
    {
        //ID REQUIRED FOR MONGODB
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Maps to the MongoDB _id field
        public required string modelURN { get; set; }
        public List<ModelPart> Parts { get; set; }

        public ModelPartInfo()
        {
            Parts = new List<ModelPart>();
        }
    }


    //Need while creating a model
    public class ModelPartInfoDTO
    {
        public required string modelURN { get; set; }
    }

    //Need while creating a part
    public class addPartDTO
    {
        public int partId { get; set; }
    }

    public class addCommentDTO
    {   
        public required string key { get; set; }
        public required string value { get; set; }
    }

}