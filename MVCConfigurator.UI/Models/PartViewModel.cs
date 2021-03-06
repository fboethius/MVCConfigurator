﻿using MVCConfigurator.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCConfigurator.UI.Models
{
    public class PartViewModel
    {
        public int ProductId { get; set; }
        public PartModel PartDetails {get; set;}
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IList<PartModel> ExistingParts { get; set; }
    }

    public class PartModel
    {
        public PartModel()
        {

        }

        public PartModel(Part part)
        {
            Id = part.Id;
            Name = part.Name;
            Price = part.Price;
            LeadTime = part.LeadTime;
            StockKeepingUnit = part.StockKeepingUnit;
            Image = part.ImagePath != null ? new PartImage(part.ImagePath):new PartImage();
            CategoryId = part.Category.Id;
            Category = part.Category.Name;

            IsIncompatible = false;
            IsSelected = false;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int LeadTime { get; set; }
        public string StockKeepingUnit { get; set; }
        public PartImage Image { get; set; }
        //public string ImagePath { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public bool IsIncompatible { get; set; }
        public bool IsSelected { get; set; }
    }
    public class PartImage
    {
        public PartImage()
        {
            
        }
        public PartImage(string imagePath)
        {
            PartImagePath = imagePath;

        }
        public string PartImagePath { get; set; }

        [Required]
        public HttpPostedFileBase PartImageUpload { get; set; }
    }
}