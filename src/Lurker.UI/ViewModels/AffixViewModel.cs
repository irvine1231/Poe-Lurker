﻿//-----------------------------------------------------------------------
// <copyright file="AffixViewModel.cs" company="Wohs">
//     Missing Copyright information from a valid stylecop.json file.
// </copyright>
//-----------------------------------------------------------------------

namespace Lurker.UI.ViewModels
{
    using Lurker.Models;

    public class AffixViewModel
    {
        #region Fields

        private Affix _affix;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AffixViewModel"/> class.
        /// </summary>
        /// <param name="affix">The affix.</param>
        public AffixViewModel(Affix affix)
        {
            this._affix = affix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text => this._affix.Text;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public double Value => this._affix.Value;

        /// <summary>
        /// Gets the actual value.
        /// </summary>
        public string ActualValue => this._affix.ActualValue.Replace(" increased", "");

        #endregion
    }
}
