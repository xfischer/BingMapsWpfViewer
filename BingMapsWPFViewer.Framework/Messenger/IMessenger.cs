using System;

namespace BingMapsWPFViewer.Framework
{
    public interface IMessenger
    {
        /// <summary>
        /// Enregistre une m�thode de rappel, sans param�tre, execut�e lorsqu'un message sp�cifique est diffus�.
        /// </summary>
        /// <param name="message">Le message sp�cifique.</param>
        /// <param name="callback">La m�thode de rappel � executer.</param>
        void Register(string message, Action callback);
        /// <summary>
        /// Enregistre une m�thode de rappel, avec un param�tre, execut�e lorsqu'un message sp�cifique est diffus�.
        /// </summary>
        /// <param name="message">Le message sp�cifique.</param>
        /// <param name="callback">La m�thode de rappel � executer.</param>
        void Register<T>(string message, Action<T> callback);
        /// <summary>
        /// Diffuse un message avec un param�tre et informe chaque objet qui s'est enregistr� pour ce message sp�cifique.
        /// </summary>
        /// <param name="message">Le message � diffuser.</param>
        /// <param name="parameter">Le param�tre joint au message diffus�.</param>
        void NotifyColleagues(string message, object parameter);
        /// <summary>
        /// Diffuse un message et informe chaque objet qui s'est enregistr� pour ce message sp�cifique.
        /// </summary>
        /// <param name="message">Le message � diffuser.</param>
        void NotifyColleagues(string message);
    }
}
