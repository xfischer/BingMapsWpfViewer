using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Framework.IOC
{
    /// <summary>
    /// Localisateur de service très simple.
    /// </summary>
    public class ServiceLocator
    {
        #region Singleton implementation
        private ServiceLocator() { }
        public static ServiceLocator Instance
        {
            get { return Nested.instance; }
        }

        class Nested
        {
            static Nested() { }
            internal static readonly ServiceLocator instance = new ServiceLocator();
        }
        #endregion


        private Dictionary<Type, object> container = new Dictionary<Type, object>();

        /// <summary>
        /// Permet d'enregistrer un élément dans le service locator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementToRegister">L'élément à enregistrer.</param>
        public void Register<T>(T elementToRegister)
        {
            container.Add(typeof(T), elementToRegister);
        }

        /// <summary>
        /// Retourne une instance du type fournit en paramètre.
        /// Une ArgumentException est levé si aucun service de ce type n'est enregistré.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Retrieve<T>()
        {
            object retrieved;
            if (container.TryGetValue(typeof(T), out retrieved))
                return (T)retrieved;

            return default(T);
            //throw
            //    new ArgumentException("Impossible de retrouver le service de type "
            //        + typeof(T).FullName);
        }

        /// <summary>
        /// Désenregistre un type donné.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Unregister<T>()
        {
            if (container.ContainsKey(typeof(T)))
            {
                container.Remove(typeof(T));
            }
        }
    }
}
