using System.Diagnostics;
using System.Text.RegularExpressions;
using CentralDenuncias.Context;
using CentralDenuncias.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralDenuncias.Controllers
{
    public class HomeController : Controller
    {

        private readonly Database _context;

        public HomeController(Database context)
        {
            _context = context;
        }

        // GET: denuncia
        public async Task<IActionResult> Index(string link)
        {
            var denuncias = _context.denuncia.AsQueryable();

            if (!string.IsNullOrEmpty(link))
            {
                denuncias = denuncias.Where(d => d.link_membro.Contains(link));
            }

            // Passando os parâmetros para a View
            ViewData["Link"] = link;

            return View(await denuncias.ToListAsync());
        }

        // GET: denuncia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var denuncia = await _context.denuncia
                .FirstOrDefaultAsync(m => m.id == id);

            string fileId = ExtractFileId(denuncia.provas);

            if (!string.IsNullOrEmpty(fileId))
            {
                // Gerar o novo link
                string newLink = $"https://drive.google.com/uc?export=download&id={fileId}";

                // Atualizar o link no banco de dados
                denuncia.provas = newLink;
            }

            if (denuncia == null)
            {
                return NotFound();
            }

            return View(denuncia);
        }

        static string ExtractFileId(string url)
        {
            var match = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        // GET: denuncia/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: denuncia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,link_membro,descricao,provas,staffer,data_criacao,data_alteracao")] denuncia denuncia)
        {
            denuncia.data_criacao = DateTime.UtcNow;
            denuncia.data_alteracao = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(denuncia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(denuncia);
        }

        // GET: denuncia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var denuncia = await _context.denuncia.FindAsync(id);
            if (denuncia == null)
            {
                return NotFound();
            }
            return View(denuncia);
        }

        // POST: denuncia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,link_membro,descricao,provas,staffer, data_criacao")] denuncia denuncia)
        {
            denuncia.data_criacao = denuncia.data_criacao.ToUniversalTime();
            denuncia.data_alteracao = DateTime.UtcNow;

            if (id != denuncia.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(denuncia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!denunciaExists(denuncia.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(denuncia);
        }

        // GET: denuncia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var denuncia = await _context.denuncia
                .FirstOrDefaultAsync(m => m.id == id);
            if (denuncia == null)
            {
                return NotFound();
            }

            return View(denuncia);
        }

        // POST: denuncia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var denuncia = await _context.denuncia.FindAsync(id);
            if (denuncia != null)
            {
                _context.denuncia.Remove(denuncia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool denunciaExists(int id)
        {
            return _context.denuncia.Any(e => e.id == id);
        }
    }
}
