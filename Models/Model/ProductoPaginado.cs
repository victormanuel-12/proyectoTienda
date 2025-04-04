using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace proyectoTienda.Models
{
    public class ProductoPaginado<T> where T : class
    {
        public List<T> Items { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        public ProductoPaginado(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalItems = count;
            PageSize = pageSize;
            CurrentPage = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
        
        public static async Task<ProductoPaginado<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            // Primero obtenemos el conteo total para asegurar que no haya problemas
            var count = await source.CountAsync();
            
            // Luego obtenemos los elementos paginados
            var items = await source.Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
            
            return new ProductoPaginado<T>(items, count, pageIndex, pageSize);
        }
        
        // Para listas en memoria (no IQueryable)
        public static ProductoPaginado<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            
            return new ProductoPaginado<T>(items, count, pageIndex, pageSize);
        }
    }
}