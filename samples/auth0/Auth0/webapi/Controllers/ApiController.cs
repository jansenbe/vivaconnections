using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIApplication.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        [HttpGet]
        [Route("public")]
        public IActionResult Public()
        {
            return Json(new
            {
                Message = "Hello from a public endpoint! You don't need to be authenticated to see this."
            });
        }

        [HttpGet]
        [Route("private")]
        [Authorize]
        public IActionResult Private()
        {
            //return Json(new
            //{
            //    Message = "Hello from a private endpoint! You need to be authenticated to see this."
            //});

            return Json(new List<Order> {
                new Order {
                    Id = 1,
                    OrderDate = new DateTime(2016, 1, 6),
                    Region = "east",
                    Rep = "Jones",
                    Item = "Pencil",
                    Units = 95,
                    UnitCost = 1.99,
                    Total = 189.05
                },
                new Order {
                    Id = 2,
                    OrderDate = new DateTime(2016, 1, 23),
                    Region = "central",
                    Rep = "Kivell",
                    Item = "Binder",
                    Units = 50,
                    UnitCost = 19.99,
                    Total = 999.50
                },
                new Order {
                    Id = 3,
                    OrderDate = new DateTime(2016, 2, 9),
                    Region = "central",
                    Rep = "Jardine",
                    Item = "Pencil",
                    Units = 36,
                    UnitCost = 4.99,
                    Total = 179.64
                },
                new Order {
                    Id = 4,
                    OrderDate = new DateTime(2016, 2, 26),
                    Region = "central",
                    Rep = "Gill",
                    Item = "Pen",
                    Units = 27,
                    UnitCost = 19.99,
                    Total = 539.73
                },
                new Order {
                    Id = 5,
                    OrderDate = new DateTime(2016, 3, 15),
                    Region = "west",
                    Rep = "Sorvino",
                    Item = "Pencil",
                    Units = 56,
                    UnitCost = 2.99,
                    Total = 167.44
                }
            });
        }

        [HttpGet]
        [Route("private-scoped")]
        [Authorize("read:products")]
        public IActionResult Scoped()
        {
            //return Json(new
            //{
            //    Message = "Hello from a private endpoint! You need to be authenticated and have a scope of read:products to see this."
            //});

            return Json(new List<Order> {
                new Order {
                    Id = 1,
                    OrderDate = new DateTime(2016, 1, 6),
                    Region = "east",
                    Rep = "Jones",
                    Item = "Pencil",
                    Units = 95,
                    UnitCost = 1.99,
                    Total = 189.05
                },
                new Order {
                    Id = 2,
                    OrderDate = new DateTime(2016, 1, 23),
                    Region = "central",
                    Rep = "Kivell",
                    Item = "Binder",
                    Units = 50,
                    UnitCost = 19.99,
                    Total = 999.50
                },
                new Order {
                    Id = 3,
                    OrderDate = new DateTime(2016, 2, 9),
                    Region = "central",
                    Rep = "Jardine",
                    Item = "Pencil",
                    Units = 36,
                    UnitCost = 4.99,
                    Total = 179.64
                },
                new Order {
                    Id = 4,
                    OrderDate = new DateTime(2016, 2, 26),
                    Region = "central",
                    Rep = "Gill",
                    Item = "Pen",
                    Units = 27,
                    UnitCost = 19.99,
                    Total = 539.73
                },
                new Order {
                    Id = 5,
                    OrderDate = new DateTime(2016, 3, 15),
                    Region = "west",
                    Rep = "Sorvino",
                    Item = "Pencil",
                    Units = 56,
                    UnitCost = 2.99,
                    Total = 167.44
                }
            });
        }


        /// <summary>
        /// This is a helper action. It allows you to easily view all the claims of the token
        /// </summary>
        /// <returns></returns>
        [HttpGet("claims")]
        public IActionResult Claims()
        {
            return Json(User.Claims.Select(c =>
                new
                {
                    c.Type,
                    c.Value
                }));
        }
    }
}
