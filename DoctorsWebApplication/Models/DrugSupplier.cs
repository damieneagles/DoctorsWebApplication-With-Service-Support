﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DoctorsWebApplication.Models;

[Table("DrugSupplier")]
public partial class DrugSupplier
{
    [Key]
    public int DrugSupplierId { get; set; }

    public int SupplierId { get; set; }

    public int DrugId { get; set; }

    [Column(TypeName = "money")]
    public decimal RRP { get; set; }

    [Column(TypeName = "money")]
    public decimal WholeSalePrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [Required]
    [StringLength(256)]
    public string ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Required]
    [StringLength(256)]
    public string CreatedBy { get; set; }

    public Guid rowguid { get; set; }

    [ForeignKey("DrugId")]
    [InverseProperty("DrugSuppliers")]
    public virtual Drug Drug { get; set; }

    [ForeignKey("SupplierId")]
    [InverseProperty("DrugSuppliers")]
    public virtual Supplier Supplier { get; set; }
}