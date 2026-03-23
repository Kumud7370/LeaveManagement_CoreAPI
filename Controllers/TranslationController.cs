using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Translation;
using AttendanceManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly IMongoCollection<Translation> _collection;

        public TranslationController(IMongoDatabase database)
        {
            _collection = database.GetCollection<Translation>("translations");
        }

        // GET /api/Translation/flat?lang=mr  — used by Angular LanguageService
        [HttpGet("flat")]
        [AllowAnonymous]
        public async Task<ActionResult<Dictionary<string, string>>> GetFlat(
            [FromQuery] string lang = "mr")
        {
            var filterDef = Builders<Translation>.Filter.Eq("IsDeleted", false);
            var all = await _collection.Find(filterDef).ToListAsync();

            var flat = new Dictionary<string, string>();
            foreach (var t in all)
            {
                string? value = lang switch
                {
                    "en" when !string.IsNullOrWhiteSpace(t.En) => t.En,
                    "hi" when !string.IsNullOrWhiteSpace(t.Hi) => t.Hi,
                    _ => t.Mr
                };
                if (!string.IsNullOrWhiteSpace(value))
                    flat[t.Key] = value;
            }
            return Ok(flat);
        }

        // GET /api/Translation?ns=employee&page=1&pageSize=50
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetAll(
            [FromQuery] string? ns,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var filterDef = Builders<Translation>.Filter.Eq("IsDeleted", false);
            if (!string.IsNullOrWhiteSpace(ns))
                filterDef &= Builders<Translation>.Filter.Eq(t => t.Namespace, ns);

            var total = await _collection.CountDocumentsAsync(filterDef);
            var items = await _collection
                .Find(filterDef)
                .SortBy(t => t.Namespace).ThenBy(t => t.Key)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(ApiResponseDto<object>.SuccessResponse(new
            {
                Items = items.Select(t => MapToDto(t)).ToList(),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            }));
        }

        [HttpGet("namespaces")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetNamespaces()
        {
            var filterDef = Builders<Translation>.Filter.Eq("IsDeleted", false);
            var items = await _collection.Find(filterDef)
                .Project(t => t.Namespace).ToListAsync();
            return Ok(items.Distinct().OrderBy(n => n).ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<TranslationResponseDto>>> Create(
            [FromBody] CreateTranslationDto dto)
        {
            var existingFilter = Builders<Translation>.Filter.And(
                Builders<Translation>.Filter.Eq(t => t.Key, dto.Key),
                Builders<Translation>.Filter.Eq("IsDeleted", false));

            if (await _collection.Find(existingFilter).AnyAsync())
                return BadRequest(ApiResponseDto<TranslationResponseDto>.ErrorResponse(
                    "Translation key already exists"));

            var translation = new Translation
            {
                Key = dto.Key.Trim(),
                Namespace = dto.Namespace.Trim(),
                Mr = dto.Mr.Trim(),
                En = string.IsNullOrWhiteSpace(dto.En) ? null : dto.En.Trim(),
                Hi = string.IsNullOrWhiteSpace(dto.Hi) ? null : dto.Hi.Trim(),
                UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(translation);
            return Ok(ApiResponseDto<TranslationResponseDto>.SuccessResponse(
                MapToDto(translation), "Translation created successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<TranslationResponseDto>>> Update(
            string id, [FromBody] UpdateTranslationDto dto)
        {
            var filterDef = Builders<Translation>.Filter.Eq(t => t.Id, id);
            var translation = await _collection.Find(filterDef).FirstOrDefaultAsync();

            if (translation == null)
                return NotFound(ApiResponseDto<TranslationResponseDto>.ErrorResponse(
                    "Translation not found"));

            translation.Mr = dto.Mr.Trim();
            translation.En = string.IsNullOrWhiteSpace(dto.En) ? null : dto.En.Trim();
            translation.Hi = string.IsNullOrWhiteSpace(dto.Hi) ? null : dto.Hi.Trim();
            translation.UpdatedAt = DateTime.UtcNow;
            translation.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _collection.ReplaceOneAsync(filterDef, translation);
            return Ok(ApiResponseDto<TranslationResponseDto>.SuccessResponse(
                MapToDto(translation), "Translation updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(string id)
        {
            var update = Builders<Translation>.Update
                .Set("IsDeleted", true)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                Builders<Translation>.Filter.Eq(t => t.Id, id), update);

            if (result.ModifiedCount == 0)
                return NotFound(ApiResponseDto<bool>.ErrorResponse("Translation not found"));

            return Ok(ApiResponseDto<bool>.SuccessResponse(true, "Translation deleted successfully"));
        }

        // POST /api/Translation/seed — one-time bulk import
        [HttpPost("seed")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<object>>> Seed(
            [FromBody] List<CreateTranslationDto> items)
        {
            int created = 0, skipped = 0;
            foreach (var dto in items)
            {
                var existingFilter = Builders<Translation>.Filter.And(
                    Builders<Translation>.Filter.Eq(t => t.Key, dto.Key),
                    Builders<Translation>.Filter.Eq("IsDeleted", false));

                if (await _collection.Find(existingFilter).AnyAsync()) { skipped++; continue; }

                await _collection.InsertOneAsync(new Translation
                {
                    Key = dto.Key.Trim(),
                    Namespace = dto.Namespace.Trim(),
                    Mr = dto.Mr.Trim(),
                    En = string.IsNullOrWhiteSpace(dto.En) ? null : dto.En.Trim(),
                    Hi = string.IsNullOrWhiteSpace(dto.Hi) ? null : dto.Hi.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                created++;
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(new
            {
                Created = created,
                Skipped = skipped,
                Message = $"{created} keys added, {skipped} already existed."
            }));
        }

        private static TranslationResponseDto MapToDto(Translation t) => new()
        {
            Id = t.Id,
            Key = t.Key,
            Namespace = t.Namespace,
            Mr = t.Mr,
            En = t.En,
            Hi = t.Hi,
            UpdatedAt = t.UpdatedAt ?? t.CreatedAt
        };
    }
}