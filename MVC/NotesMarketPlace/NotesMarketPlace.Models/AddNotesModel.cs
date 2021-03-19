﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class AddNotesModel
    {
        [Key]
        public int SellerNotesID { get; set; }
        public int SellerID { get; set; }
        public int Status { get; set; }
        public int? ActionedBy { get; set; }
        public string AdminRemarks { get; set; }
        public DateTime? PublishedDate { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Select Category")]
        public int Category { get; set; }

        public string DisplayPicture { get; set; }

        //[Required(ErrorMessage = "Please Select File")]
        //[DataType(DataType.Upload)]
        //[Display(Name = "Upload File")]
        public string FilePath { get; set; }

        public string FileName { get; set; }
        public int? NoteType { get; set; }
        public int? NumberOfPages { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string UniversityName { get; set; }
        public int? Country { get; set; }

        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }

        [Required(ErrorMessage = "Please Select Any One")]
        public bool IsPaid { get; set; }
        public decimal? SellingPrice { get; set; }
        public string NotesPreview { get; set; }

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public SellerNotesAttachements SellerNotesAttachements { get; set; }

        public NoteCategoriesModel NoteCategoriesList { get; set; }

        public NoteTypesModel NoteTypeList { get; set; }

        public CountriesModel CountryList { get; set; }

        public HttpPostedFileBase NoteDisplayPicturePath { get; set; }

        [Required(ErrorMessage = "Please select file.")]
        public HttpPostedFileBase NoteUploadFilePath { get; set; }

        public HttpPostedFileBase NotePreviewFilePath { get; set; }
    }
}
