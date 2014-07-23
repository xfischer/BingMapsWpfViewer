using System;

namespace BingMapsWPFViewer.Framework
{
    public interface IMessenger
    {
        /// <summary>
        /// Enregistre une méthode de rappel, sans paramètre, executée lorsqu'un message spécifique est diffusé.
        /// </summary>
        /// <param name="message">Le message spécifique.</param>
        /// <param name="callback">La méthode de rappel à executer.</param>
        void Register(string message, Action callback);
        /// <summary>
        /// Enregistre une méthode de rappel, avec un paramètre, executée lorsqu'un message spécifique est diffusé.
        /// </summary>
        /// <param name="message">Le message spécifique.</param>
        /// <param name="callback">La méthode de rappel à executer.</param>
        void Register<T>(string message, Action<T> callback);
        /// <summary>
        /// Diffuse un message avec un paramètre et informe chaque objet qui s'est enregistré pour ce message spécifique.
        /// </summary>
        /// <param name="message">Le message à diffuser.</param>
        /// <param name="parameter">Le paramètre joint au message diffusé.</param>
        void NotifyColleagues(string message, object parameter);
        /// <summary>
        /// Diffuse un message et informe chaque objet qui s'est enregistré pour ce message spécifique.
        /// </summary>
        /// <param name="message">Le message à diffuser.</param>
        void NotifyColleagues(string message);
    }
}
