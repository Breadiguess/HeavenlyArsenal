using NoxusBoss.Core.Autoloaders.SolynBooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Core.Autoloaders.SolynBooks
{

    /// <summary>
    /// TODO: figure out solyn book so taht i can make a rift trade and impress lucille :tm:
    /// </summary>
    public partial class SolnyBookAutoloader
    {
        //i should be executed for this

        public static readonly Dictionary<string, AutoloadableSolynBook> Books = [];

        public static AutoloadableSolynBook Create(Mod mod, LoadableBookData data)
        {
            AutoloadableSolynBook book = new AutoloadableSolynBook(data);
            mod.AddContent(book);

            Books[book.Name] = book;

            return book;
        }
    }
}
