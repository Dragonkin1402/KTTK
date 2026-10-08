using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StudentAppMongo;

// Tầng DATA: làm việc trực tiếp với MongoDB
public class StudentRepository
{
    private readonly IMongoCollection<Student> _students;

    public StudentRepository(string connectionString,
                             string databaseName = "StudentDB",
                             string collectionName = "students")
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _students = database.GetCollection<Student>(collectionName);
    }

    // Kiểm tra kết nối tới Atlas
    public async Task PingAsync()
        => await _students.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

    public async Task<List<Student>> GetAllAsync()
        => await _students.Find(_ => true).ToListAsync();

    public async Task<Student?> GetByIdAsync(string id)
        => await _students.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(Student student)
        => await _students.InsertOneAsync(student);

    public async Task<bool> UpdateAsync(Student student)
    {
        var result = await _students.ReplaceOneAsync(s => s.Id == student.Id, student);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _students.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    // Tìm gần đúng, không phân biệt hoa thường (regex "i"), đã Escape để tránh ký tự đặc biệt
    public async Task<List<Student>> SearchByNameAsync(string keyword)
    {
        var filter = Builders<Student>.Filter.Regex(
            s => s.Name, new BsonRegularExpression(Regex.Escape(keyword), "i"));
        return await _students.Find(filter).ToListAsync();
    }

    public async Task<List<Student>> SearchByAddressAsync(string keyword)
    {
        var filter = Builders<Student>.Filter.Regex(
            s => s.Address, new BsonRegularExpression(Regex.Escape(keyword), "i"));
        return await _students.Find(filter).ToListAsync();
    }

    public async Task<List<Student>> SearchByGradeAsync(double grade)
        => await _students.Find(s => s.Grade == grade).ToListAsync();
}
