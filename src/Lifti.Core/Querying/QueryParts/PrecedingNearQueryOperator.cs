﻿using System;
using System.Globalization;

namespace Lifti.Querying.QueryParts
{
    /// <summary>
    /// Requires that the <see cref="BinaryQueryOperator.Left" /> part be before
    /// the <see cref="BinaryQueryOperator.Right"/> part, and within <see cref="PrecedingNearQueryOperator.Tolerance"/> tokens.
    /// </summary>
    public class PrecedingNearQueryOperator : BinaryQueryOperator
    {
        public PrecedingNearQueryOperator(IQueryPart left, IQueryPart right, int tolerance = 5)
            : base(left, right)
        {
            this.Tolerance = tolerance;
        }

        public override OperatorPrecedence Precedence => OperatorPrecedence.Positional;

        public int Tolerance { get; }

        public override IntermediateQueryResult Evaluate(Func<IIndexNavigator> navigatorCreator, IQueryContext queryContext)
        {
            return this.Left.Evaluate(navigatorCreator, queryContext)
                .CompositePositionalIntersect(
                this.Right.Evaluate(navigatorCreator, queryContext), 
                    0, 
                    this.Tolerance);
        }

        public override string ToString()
        {
            var toleranceText = this.Tolerance == 5 ? string.Empty : this.Tolerance.ToString(CultureInfo.InvariantCulture);
            return $"{this.Left} ~{toleranceText}> {this.Right}";
        }
    }
}
