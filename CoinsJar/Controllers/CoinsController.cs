using CoinJar.Entities;
using CoinJar.Interfaces;
using CoinJar.Utilities;
using CoinJar.ViewModels;
using CoinJar.ViewModels.Requests;
using CoinJar.ViewModels.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoinJar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinsController : ControllerBase
    {
        private readonly IUnitOfWork dbContext;

        public CoinsController(IUnitOfWork unitOfWork)
        {
            dbContext = unitOfWork;
        }


        /// <summary>
        /// To Add New Coin
        /// </summary>
        /// <param name = "request" ></param >
        /// <returns></returns >
        [Route("add")]
        [HttpPost]
        public async Task<CJResponse> AddNewCoin([FromBody] AddCoinRequest request)
        {
            CJResponse response = new CJResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCount = await dbContext.Coins.GetTotalVolume();
                    if (existingCount < 42 && (existingCount + request.Coin.Volume) <= 42)
                    {
                        dbContext.Coins.Add(new Coin()
                        {
                            Amount = request.Coin.Amount,
                            Volume = request.Coin.Volume
                        });
                        dbContext.Save();

                        response = new CJResponse()
                        {
                            Message = "New Coin added successfully..!",
                            Status = StausCodes.Success.EnumToNumber()
                        };
                    }
                    else
                    {
                        response = new CJResponse()
                        {
                            Message = "Coins volume exceeded the max count..!",
                            Status = StausCodes.Invalid.EnumToNumber()
                        };
                    }
                }
                catch (Exception ex)
                {
                    response = new CJResponse()
                    {
                        Message = $"Failed to add new coin, Error : {ex.Message}",
                        Status = StausCodes.Error.EnumToNumber()
                    };
                }
            }
            else
            {
                response = new CJResponse()
                {
                    Message = "Coin Amount or Volume is missing",
                    Status = StausCodes.Invalid.EnumToNumber()
                };
            }
            return response;
        }


        /// <summary>
        /// To Add New Coin
        /// </summary>
        /// <param name = "request" ></param >
        /// <returns></returns >
        [Route("get-total-count")]
        [HttpGet]
        public async Task<TotalCountResponse> GetTotalAmount()
        {
            TotalCountResponse response = new TotalCountResponse();

            try
            {
                response = new TotalCountResponse()
                {
                    Response = new CJResponse()
                    {
                        Message = "Total count retrived successfully..!",
                        Status = StausCodes.Success.EnumToNumber()
                    },
                    TotalAmount = await dbContext.Coins.GetTotalAmount()
                };
            }
            catch (Exception ex)
            {
                response = new TotalCountResponse()
                {
                    Response = new CJResponse()
                    {
                        Message = $"Failed to add new coin, Error : {ex.Message}",
                        Status = StausCodes.Error.EnumToNumber()
                    },
                    TotalAmount = 0

                };
            }
            return response;
        }

        /// <summary>
        /// To Reset
        /// </summary>
        /// <param name = "request" ></param >
        /// <returns></returns >
        [Route("reset")]
        [HttpGet]
        public async Task<CJResponse> ResetAmount()
        {
            CJResponse response = new CJResponse();

            try
            {
                await dbContext.Coins.Reset();
              
                response = new CJResponse()
                {
                    Message = "Reset amount for existing records..!",
                    Status = StausCodes.Success.EnumToNumber()
                };
            }
            catch (Exception ex)
            {
                response = new CJResponse()
                {
                    Message = $"Failed to reset, Error : {ex.Message}",
                    Status = StausCodes.Error.EnumToNumber()
                };
            }
            return response;
        }
    }
}
