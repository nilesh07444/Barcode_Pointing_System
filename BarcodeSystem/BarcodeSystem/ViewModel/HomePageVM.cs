using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarcodeSystem
{
    public class HomePageVM
    {
        public List<HomeImageVM> HomePageSlider { get; set; }

        public List<ProductVM> PopularProducts { get; set; }
    }
}