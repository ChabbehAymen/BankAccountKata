
using System.Collections;

namespace BankAccountKata
{
    internal class Transactions : IEnumerable<Transaction>
    {
        readonly int MaximumRegularTransactionCount = 50;
        readonly List<Transaction> transactions = [];

        internal void Add(Transaction transaction)
        {
            transactions.Add(transaction);
        }


        private bool TransactionsNeedAggregation()
        {
            return transactions.Count > MaximumRegularTransactionCount;
        }

        private IEnumerable<Transaction> GetTransactionsAfterAggregation()
        {
            var aggregateTransaction = AggregateTransactionsNeedingAggregation();

            return CombineAggregateWithRemainingRegularTransactions(aggregateTransaction);
        }

        private Transaction AggregateTransactionsNeedingAggregation()
        {
            var transactionsToAggregate = FilterTransactionsNeedingAggregation();
            var aggregatedTransaction = CreateAggregatedTransactionFrom(transactionsToAggregate);

            return aggregatedTransaction;
        }

        private Transaction[] FilterTransactionsNeedingAggregation()
        {
            var transactionsToAggregateCount = GetTransactionsToAggregateCount();
            var transactionsToAggregate = transactions.Take(transactionsToAggregateCount).ToArray();

            return transactionsToAggregate;
        }

        private int GetTransactionsToAggregateCount()
        {
            return transactions.Count - MaximumRegularTransactionCount;
        }

        private static Transaction CreateAggregatedTransactionFrom(Transaction[] transactions)
        {
            return new(transactions.Sum(t => t.Amount),
                       transactions.Last().Date,
                       transactions.Last().Balance);
        }

        private IEnumerable<Transaction> GetTransactions()
        {
            if (TransactionsNeedAggregation())
            {
                return GetTransactionsAfterAggregation();
            }
            else
            {
                return transactions;
            }
        }
        private IEnumerable<Transaction> CombineAggregateWithRemainingRegularTransactions(Transaction aggregatedTransaction)
        {
            var regularTransactions = GetTransactionsNotNeedingAggregation();
            Transaction[] combinedTransactions = [aggregatedTransaction, .. regularTransactions];

            return combinedTransactions;
        }

        private IEnumerable<Transaction> GetTransactionsNotNeedingAggregation()
        {
            var transactionsToAggregateCount = GetTransactionsToAggregateCount();
            return transactions.Skip(transactionsToAggregateCount);
        }

        public IEnumerator<Transaction> GetEnumerator()
        {
            return GetTransactions().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
