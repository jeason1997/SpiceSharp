﻿namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This interface can read tokens
    /// </summary>
    public abstract class Reader : TokenConstants
    {
        /// <summary>
        /// The reader type
        /// </summary>
        public StatementType Type { get; private set; }

        /// <summary>
        /// An optional identifier that can be used by the ReaderCollection
        /// to find the right reader
        /// </summary>
        public string Identifier { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of reader</param>
        protected Reader(StatementType type)
        {
            Type = type;
        }

        /// <summary>
        /// Get the generated object by the reader
        /// </summary>
        public object Generated { get; protected set; } = null;

        /// <summary>
        /// Read a statement
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns>True if reading succeeded</returns>
        public abstract bool Read(string type, Statement st, Netlist netlist);
    }
}