using System.Diagnostics;
using System.Net;
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
            string adm = HttpContext.Session.GetString("adm");

            if (!string.IsNullOrEmpty(link))
            {
                denuncias = denuncias.Where(d => d.link_membro.Contains(link));
            }

            // Passando os parâmetros para a View
            ViewData["Link"] = link;
            ViewBag.adm = adm;

            return View(await denuncias.ToListAsync());
        }

        // Página de login
        public IActionResult Login()
        {
            return View();
        }

        // Ação de login, que valida a senha
        [HttpPost]
        public async Task<IActionResult> Login(string senha)
        {
            var usuarios = new Dictionary<string, Tuple<string, string>>
            {
                { "3301", new Tuple<string, string>("Totoro", "true") },
                { "9ijq", new Tuple<string, string>("Aeon", "false") },
                { "lgcy", new Tuple<string, string>("Agnes", "false") },
                { "69zc", new Tuple<string, string>("Deku", "false") },
                { "01dm", new Tuple<string, string>("Gab", "true") },
                { "lt07", new Tuple<string, string>("Yuri", "false") },
                { "2bfn", new Tuple<string, string>("Moon", "false") },
                { "yy9r", new Tuple<string, string>("ForUs", "false") },
                { "cnn9", new Tuple<string, string>("Olivia", "false") },
                { "7tyt", new Tuple<string, string>("Rychard", "false") },
                { "adnb", new Tuple<string, string>("Manoella", "false") },
                { "u566", new Tuple<string, string>("Akashy", "false") },
                { "1g6l", new Tuple<string, string>("Misa", "false") }
            };

            if (usuarios.ContainsKey(senha))
            {
                var usuario = usuarios[senha];
                // Armazena o cookie para indicar que o usuário está autenticado
                Response.Cookies.Append("SenhaAutenticada", "true", new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(30) // Tempo que o cookie ficará válido
                });
                var ipInfos = new ip() { };
                await unicLogin(usuario.Item1, usuario.Item2, ipInfos);
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Erro = "Senha incorreta!";
                return View("Login");
            }
        }


        [HttpPost]
        public async Task unicLogin(string nomeStaffer, string isAdm, ip ipInfos)
        {

            // Obtém o IP do cliente a partir do cabeçalho X-Forwarded-For, se disponível
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();

            // Caso o cabeçalho não esteja presente, usa o IP direto da conexão
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            ipInfos.nome_staffer = nomeStaffer;
            ipInfos.ip_staffer = ipAddress;
            ipInfos.data_acesso = DateTime.UtcNow;


            _context.Add(ipInfos);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("adm", isAdm);
        }

        // GET: denuncia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string adm = HttpContext.Session.GetString("adm");
            ViewBag.adm = adm;
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

        [HttpPost]
        public IActionResult AlterarStatus(int id, string status)
        {
            // Buscar a denúncia no banco de dados pelo ID
            var denuncia = _context.denuncia.FirstOrDefault(d => d.id == id);

            if (denuncia == null)
            {
                // Se a denúncia não for encontrada, retornar erro ou redirecionar
                return NotFound();
            }

            // Atualizar o status da denúncia
            denuncia.status = status;

            // Salvar as mudanças no banco de dados
            _context.SaveChanges();

            // Redirecionar para a página de detalhes da denúncia
            return RedirectToAction("Details", new { id = denuncia.id });
        }

        [HttpPost]
        public IActionResult AtualizarNome(int id, string nome_membro)
        {
            // Buscar a denúncia no banco de dados pelo ID
            var denuncia = _context.denuncia.FirstOrDefault(d => d.id == id);

            if (denuncia == null)
            {
                // Se a denúncia não for encontrada, retornar erro ou redirecionar
                return NotFound();
            }

            // Atualizar o nome do membro
            denuncia.nome_membro = nome_membro;

            // Salvar as mudanças no banco de dados
            _context.SaveChanges();

            // Redirecionar para a página de detalhes da denúncia
            return RedirectToAction("Details", new { id = denuncia.id });
        }

        [HttpPost]
        public IActionResult Deletar(int id)
        {
            var denuncia = _context.denuncia.FirstOrDefault(d => d.id == id);

            if (denuncia == null)
            {
                // Se a denúncia não for encontrada, redireciona para uma página de erro ou para a lista de denúncias
                return NotFound();
            }

            // Remover o registro do banco de dados
            _context.denuncia.Remove(denuncia);
            _context.SaveChanges();

            // Após deletar, redirecionar de volta para a página principal ou lista de registros
            return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> Create([Bind("id,nome_membro,link_membro,descricao,provas,staffer,denuncia_permanente,status,data_criacao,data_alteracao")] denuncia denuncia)
        {
            denuncia.data_criacao = DateTime.UtcNow;
            denuncia.data_alteracao = DateTime.UtcNow;
            denuncia.status = "pendente";

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
