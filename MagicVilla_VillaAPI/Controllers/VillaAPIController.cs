using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        //Add Dependency Injection for ApplicationDbContext
        //As long as we dont use Repository pattern
        //Later we supstitute with Repository pattern

        //private readonly ApplicationDbContext _db;

        //Add Dependency Injection for AutoMapper
        private readonly IMapper _mapper;
        //Add Dependency Injection for Repository
        private readonly IVillaRepository _dbVilla;
        //Adding ApiResponse
        protected ApiResponse _response;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            // _db= db;
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._response = new();

        }

        //to add logger to our API with Dependency Injection
        //private readonly ILogger<VillaAPIController> _logger;


        //public VillaAPIController(ILogger<VillaAPIController> logger)
        //{
        //    _logger = logger;
        //}

        //to create Custom Logging
        //private readonly ILogging _logger;

        //public VillaAPIController(ILogging logger)
        //{
        //    _logger = logger;
        //}

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetVillas()
        {
            //we could use instead of ApiResponse  IEnumerable<VillaDTO>

            // _logger.LogInformation("Getting all Villas");   //serilog
            //_logger.Log("Getting all Villas","");              //custom Log

            //With ApplicationDb
            //IEnumerable<Villa> villaList =await _db.Villas.ToListAsync();

            //With Repository Pattern
            IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();

            ////automatically makes the map  _mapper.Map<List<VillaDTO>>(villaList);
            try
            {
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type=typeof(VillaDTO))]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(400)]
        public async Task<ActionResult<ApiResponse>> GetVilla(int id)
        {
            //we could use instead of ApiResponse  <VillaDTO>

            try
            {

                if (id == 0)
                {
                    //_logger.LogError("Get Villa Error with Id " + id);    //serilog
                    //_logger.Log("Get Villa Error with Id " + id,"error");   //custom Log

                    _response.StatusCode = HttpStatusCode.BadRequest;                    
                    return BadRequest(_response);
                }
                //With ApplicationDb
                //var villa =await  _db.Villas.FirstOrDefaultAsync(u => u.Id == id);

                //With Repository Pattern
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                //Before adding ApiResponse
                // return Ok(_mapper.Map<VillaDTO>(villa));

                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            //we use this if     [ApiController]  decorator does not exist
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //The name of Villa should be unique
            //With ApplicationDb
            //var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower());
            try
            {
                //With repository Pattern
                var villaT = await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower());

                if (villaT != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exists");
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }
                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}


                Villa villa = _mapper.Map<Villa>(createDTO);

                //Villa villa = new()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //    ImageUrl = createDTO.ImageUrl,
                //    Name = createDTO.Name,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft,
                //};

                //With ApplicationDb
                //await _db.Villas.AddAsync(model);
                //await _db.SaveChangesAsync();

                //With Repository Pattern
                _dbVilla.CreateAsync(villa);

                //Without ApiResponse
                //return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);
                _response.Result = _mapper.Map<List<VillaDTO>>(villa);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        public async Task<ActionResult<ApiResponse>> DeleteVilla(int id)
        {
            //before ApiResponse : Task<IActionResult> DeleteVilla(int id)

            //With ApplicationDb
            //if (id == 0)
            //{
            //    return BadRequest();
            //}
            //var villa =await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            //if (villa == null)
            //{
            //    return NotFound();
            //}
            //_db.Villas.Remove(villa);
            //await _db.SaveChangesAsync();
            //return NoContent();
            try
            {
                //With Repository Pattern
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                _dbVilla.RemoveAsync(villa);

                //Before using ApiResponse
                //return NoContent();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            //before using ApiResponse Task<IActionResult> 

            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }
                //var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
                //villa.Name = villaDTO.Name;
                //villa.Sqft = villaDTO.Sqft;
                //villa.Occupancy = villaDTO.Occupancy;

                Villa model = _mapper.Map<Villa>(updateDTO);

                //Villa model = new()
                //{
                //    Amenity = updateDTO.Amenity,
                //    Details = updateDTO.Details,
                //    Id = updateDTO.Id,
                //    ImageUrl = updateDTO.ImageUrl,
                //    Name = updateDTO.Name,
                //    Occupancy = updateDTO.Occupancy,
                //    Rate = updateDTO.Rate,
                //    Sqft = updateDTO.Sqft,
                //};

                //With ApplicationDb
                //_db.Villas.Update(model);
                //await _db.SaveChangesAsync();
                //return NoContent();

                //With Repository Pattern
                await _dbVilla.UpdateAsync(model);

                //Before ApiResponse
                //return NoContent();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            //With ApplicationDb
            //var villa =await  _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            //With Repository Pattern
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);


            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    Id = villa.Id,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft,
            //};
            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

            //Villa model = new()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    Id = villaDTO.Id,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft,
            //};

            //With AppicationDb
            //_db.Villas.Update(model);
            //await _db.SaveChangesAsync();

            //With Repository pattern
            await _dbVilla.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

    }
}
