﻿using Amazon.Models;
using Amazon.Models.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Amazon.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoRepository _carritoRepository;
        private readonly IVentaRepository _ventaRepository;
        public CarritoController(ICarritoRepository carritoRepository, IVentaRepository ventaRepository)
        {
            _carritoRepository = carritoRepository;
            _ventaRepository = ventaRepository;
        }

        // GET: CarritoController
        public async Task<IActionResult> Index()
        {
            if (!CheckSession("User"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (CheckSession("Admin"))
            {
                var carrito = await _carritoRepository.GetCartAdmin();
                return View(carrito);
            } else
            {
                return RedirectToAction("Details");
            }
        }

        // GET: CarritoController/Details/5
        public async Task<IActionResult> Details(int? cartID)
        {
            int cart = cartID ?? await _carritoRepository.GetCartID(Global.user.UsuarioID);
            var carrito = await _carritoRepository.GetAllCart(cart);
            var jsonString = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", jsonString);
            return View(carrito);
        }

        // GET: CarritoController/Edit/5
        public async Task<IActionResult> Edit(int cartID)
        {
            var detallesCarrito = await _carritoRepository.GetAllCart(cartID);
            return detallesCarrito == null ? NotFound() : View(detallesCarrito);
        }

        // POST: CarritoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DetallesCarrito detallesCarrito)
        {
            try
            {
                _carritoRepository.EditDetallesProductoCarrito(detallesCarrito);
                var carrito = _carritoRepository.GetAllCart(detallesCarrito.CarritoID);
                return RedirectToAction("Details", carrito);
            }
            catch
            {
                return View(detallesCarrito.CarritoID);
            }
        }
        // POST: CarritoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductoAsync(int productoID)
        {
            int cartID = await _carritoRepository.GetCartID(Global.user.UsuarioID);
            try
            {
                await _carritoRepository.RmProducto(productoID, cartID);
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Details", cartID);
            }
        }
        //Varios productos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVenta(List<int> listaID)
        {
            int userID = Global.user.UsuarioID;
            foreach(var productoID in listaID)
            {
                await _ventaRepository.AddVenta(productoID, userID);
            }
            return RedirectToAction("Index");
        }
        protected bool CheckSession(string key)
        {
            return HttpContext.Session.Keys.Contains(key);
        }

    }
}
