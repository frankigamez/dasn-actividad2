using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using DASN.Core.Data.Contexts;
using DASN.Core.Data.Models;
using DASN.Core.Data.Services;
using DASN.Core.Test.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DASN.Core.Test.SecurityTesting
{
    [TestClass]
    public class SqlInjectionTest
    {
        #region Privates
        /// <summary>
        /// Ejemplos de inyecciones para operaciones 'SELECT'
        /// </summary>
        private static readonly List<string> SelectInjectionExamples = new List<string>
        {
            "\" or 1=1 --",
            "\" or 1=1",
            "\" or 1=1; --",
            "\" or 1=1;",
            "\" or 1=1); --",
            "\" or 1=1);",

            "' or 1=1 --",
            "' or 1=1",
            "' or 1=1; --",
            "' or 1=1;",
            "' or 1=1); --",
            "' or 1=1);",

            "0 or 1=1 --",
            "0 or 1=1",
            "0 or 1=1; --",
            "0 or 1=1;",
            "0 or 1=1); --",
            "0 or 1=1);",
        };

        /// <summary>
        /// Ejemplos de inyecciones para operaciones 'INSERT'
        /// </summary>
        private static readonly List<string> InsertInjectionExamples = new List<string>
        {
            " (select sqlite_version()) ",
            "' || (select sqlite_version()) || '",
            "\" || (select sqlite_version()) || \"",
            "' || (select sqlite_version())) --",
            "\" || (select sqlite_version())) --",
        };

        private TestDbContext _context;
        private UserService _userService;
        private PostService _postService;
        private AuthService _authService;
        #endregion

        #region PreTest & PostTest
        [TestInitialize]
        public void Initialize()
        {
            //Configuramos el contexto de base de datos para test
            ContextHelper.StartContext();

            //Iniciamos el contexto de base de datos como 'TestDbContext' (sqlite)
            _context = new TestDbContext();
            _context.Posts.ToList().ForEach(x => _context.Posts.Remove(x));
            _context.Auths.ToList().ForEach(x => _context.Auths.Remove(x));
            _context.Users.ToList().ForEach(x => _context.Users.Remove(x));

            //Inyectamos el contexto en el servicio de 'Users', y metemos datos de prueba
            _userService = new UserService(_context);            
            var user1 = _userService.AddUser(email: "user1@test.com", createdAt: DateTime.Now);
            var user2 = _userService.AddUser(email: "user2@test.com", createdAt: DateTime.Now);

            //Inyectamos el contexto en el servicio de 'Posts', y metemos datos de prueba
            _postService = new PostService(_context);
            var post1 = _postService.AddPost(title: "title-post1", content: "content-post1", isPublic: false, createdAt: DateTime.Now, userId: user1.Id);
            var post2 = _postService.AddPost(title: "title-post2", content: "content-post2", isPublic: false, createdAt: DateTime.Now, userId: user1.Id);
            var post3 = _postService.AddPost(title: "title-post3", content: "content-post3", isPublic: true, createdAt: DateTime.Now, userId: user1.Id);
            var post4 = _postService.AddPost(title: "title-post4", content: "content-post4", isPublic: false, createdAt: DateTime.Now, userId: user2.Id);
            var post5 = _postService.AddPost(title: "title-post5", content: "content-post5", isPublic: true, createdAt: DateTime.Now, userId: user2.Id);

            //Inyectamos el contexto en el servicio de 'Auths', y metemos datos de prueba
            _authService = new AuthService(_context);
            var auth1 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt:DateTime.Now, userId: user1.Id);
            var auth2 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id);
            var auth3 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user1.Id);
            var auth4 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user2.Id);
            var auth5 = _authService.AddAuth(hash: Guid.NewGuid().ToString(), salt: Guid.NewGuid().ToString(), createdAt: DateTime.Now, userId: user2.Id);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _context.Dispose();
            ContextHelper.EndContext();
        }
        #endregion

        #region User Service Test
        /// <summary>
        /// Test de User->AddUser. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void UserService_AddUser()
        {
            //Como es un Insert, probamos con la batería de pruebas de InsertInjection.
            InsertInjectionExamples.ForEach(injectTry =>
            {
                //Creamos uno usando la prueba de inyección en todos los campos 'string' del modelo
                var added = _userService.AddUser(email: injectTry, createdAt: DateTime.UtcNow);
                //Obtenemos el modelo creado directamente desde el origen de datos
                var getAdded = _userService.GetUserById(added.Id);
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (added.Email != getAdded.Email && injectTry != added.Email)
                    Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {getAdded}");
            });
        }
        #endregion

        #region Post ServiceTest
        /// <summary>
        /// Test de Post->AddPost. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void PostService_AddPost()
        {
            //Como es un Insert, probamos con la batería de pruebas de InsertInjection.
            InsertInjectionExamples.ForEach(injectTry =>
            {
                //Creamos uno usando la prueba de inyección en todos los campos 'string' del modelo
                var added = _postService.AddPost(title: injectTry, content: injectTry, isPublic: false,
                    createdAt: DateTime.Now, userId: 1);
                //Obtenemos el modelo creado directamente desde el origen de datos
                var getAdded = _postService.GetPostById(added.Id);
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (added.Title != getAdded.Title && injectTry != added.Title)
                    Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {getAdded}");
                if (added.Content != getAdded.Content && injectTry != added.Content)
                    Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {getAdded}");
            });
        }

        /// <summary>
        /// Test de Post->GetPostsByUser. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void PostService_GetPostsByUser()
        {
            //Como es un Select, probamos con la batería de pruebas de SelectInjection.
            SelectInjectionExamples.ForEach(injectTry =>
            {
                //Lanzamos la consulta usando la prueba de inyección en todos los campos 'string' del modelo
                var getted = _postService.GetPostsByUser(new User {Email = injectTry, Id = 0 });
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (getted.Count > 0)
                    Assert.Fail($"ModelStateFailed! expected 0 -> getted {getted.Count}");
            });
        }
        #endregion

        #region Auth ServiceTest
        /// <summary>
        /// Test de Auth->AddAuth. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void AuthService_AddAuth()
        {
            //Como es un Insert, probamos con la batería de pruebas de InsertInjection.
            InsertInjectionExamples.ForEach(injectTry =>
            {
                //Creamos uno usando la prueba de inyección en todos los campos 'string' del modelo
                var added = _authService.AddAuth(hash: injectTry, salt: injectTry, createdAt: DateTime.Now, userId: 1);
                //Obtenemos el modelo creado directamente desde el origen de datos
                var getAdded = _authService.GetAuthById(added.Id);
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (added.Hash != getAdded.Hash && injectTry != added.Hash)
                    Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {getAdded}");
                if (added.Salt != getAdded.Salt && injectTry != added.Salt)
                    Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {getAdded}");
            });
        }

        /// <summary>
        /// Test de Auth->GetCurrentAuthByUser. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void AuthService_GetCurrentAuthByUser()
        {
            //Como es un Select, probamos con la batería de pruebas de SelectInjection.
            SelectInjectionExamples.ForEach(injectTry =>
            {
                //Lanzamos la consulta usando la prueba de inyección en todos los campos 'string' del modelo
                var getted = _authService.GetCurrentAuthByUser(new User { Email = injectTry, Id = 0});
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (getted != null)
                    Assert.Fail($"ModelStateFailed! expected null -> getted { JsonConvert.SerializeObject(getted)}");
            });
        }

        /// <summary>
        /// Test de Auth->GetAuthsByUser. Probamos posibles inyecciones sql en el servicio.
        /// </summary>
        [TestMethod]
        public void AuthService_GetAllAuthsByUser()
        {
            //Como es un Select, probamos con la batería de pruebas de SelectInjection.
            SelectInjectionExamples.ForEach(injectTry =>
            {
                //Lanzamos la consulta usando la prueba de inyección en todos los campos 'string' del modelo
                var getted = _authService.GetAuthsByUser(new User { Email = injectTry, Id = 0 });
                //Comparamos para ver que el resultado es el esperado y la inyección se ha mitigado 
                if (getted.Count > 0)
                    Assert.Fail($"ModelStateFailed! expected 0 -> getted {getted.Count}");
            });
        }
        #endregion

        /// <summary>
        /// Test de la batería de pruebas de SelectInjection
        /// </summary>
        [TestMethod]
        public void GetInjects_Test() => SelectInjectionExamples.ForEach(injectTry =>
        {
            new List<string> {"", "("}.ForEach(initSelect => 
            new List<string> {"", " and email != ''"}.ForEach(continuation =>
            new List<string> {"", "\"", "'"}.ForEach(openWhereItem =>
            {
                try
                {
                    //Preparamos la consulta SQL forzando distintos tipos de inyeccion
                    var injectTest =
                        _context.Users.SqlQuery($"{initSelect} select * from users where id = {openWhereItem}{injectTry}{continuation}");
                    //Ejecutamos la consulta
                    var results = injectTest.Count();
                    if (results == 0)
                        Assert.Fail($"ModelStateFailed! expected: >0 -> obtained: {results}");
                }
                catch (SQLiteException e)
                {
                    //Si el fallo lo captura el motor sqlite, quiere decir que 
                    //la consulta no se ha podido lanzar por error de sintaxis. DONE!
                }
            })));
        });

        /// <summary>
        /// Test de la batería de pruebas de InsertInjection
        /// </summary>
        [TestMethod]
        public void AddInjects_Test()
        {
            //Obtenemos la version de sqlite (es el dato que vamos a inyectar)
            var version = _context.Database.SqlQuery<string>("select sqlite_version()").FirstOrDefault();

            InsertInjectionExamples.ForEach(injectTry =>
            {
                try
                {
                    //Ejecutamos la consulta con la inyección
                    var injectTest = _context.Database.ExecuteSqlCommand($"insert into users (email, createdAt) values ('{injectTry}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' )");                    
                    if (injectTest == 0)
                        //Si no falla por la sintaxis, la consulta se debe haber ejecutado y haber dado como resultado '1'. FAIL!                        
                        Assert.Fail($"ModelStateFailed! expected: >0 -> obtained: {injectTest}");
                    else
                    {
                        //La consulta se ha ejecutado, obtenemos el modelo insertado
                        var last = _context.Users.ToList().Last();
                        //Comprobamos que la inyección se ha producido correctamente, y el resultado es el esperado
                        if (last.Email.Equals(version))
                            return;
                        if (last.Email != injectTry)
                            Assert.Fail($"ModelStateFailed! expected: {injectTry} -> obtained: {last.Email}");
                    }
                }
                catch (SQLiteException e)
                {
                    //Si el fallo lo captura el motor sqlite, quiere decir que 
                    //la consulta no se ha podido lanzar por error de sintaxis. DONE!
                }
            });
        }
    }
}
