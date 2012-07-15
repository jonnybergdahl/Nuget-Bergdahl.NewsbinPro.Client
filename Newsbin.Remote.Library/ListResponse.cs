using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class ListResponse: Response
    {
        protected List<string> listItems;

        public IEnumerable<string> ListItems
        {
            get { return this.listItems; }
        }

        public void AddListItem(string item)
        {
            this.listItems.Add(item);
        }

        public ListResponse(string response)
            : base(response)
        {
            this.listItems = new List<string>();
        }
    }
}
