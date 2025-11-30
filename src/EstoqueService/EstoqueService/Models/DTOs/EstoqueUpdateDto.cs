using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueService.Models.DTOs
{
    public class EstoqueUpdateDto
    {
        [Required(ErrorMessage = "Quantidade é obrigatória!")]
        public int Quantidade {get; set;}

        [Required(ErrorMessage = "Operação é obrigatória!")]
        [AllowedValues("adicionar", "remover", "definir")]
        public string Operacao { get; set;} = string.Empty;
    }
}