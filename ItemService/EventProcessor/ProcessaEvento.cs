using AutoMapper;
using ItemService.Data;
using ItemService.Dtos;
using ItemService.Models;
using System.Text.Json;

namespace ItemService.EventProcessor;

public class ProcessaEvento : IProcessaEvento
{
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProcessaEvento(IMapper mapper, IServiceScopeFactory scopeFactory)
    {
        _mapper = mapper;
        _scopeFactory = scopeFactory;
    }

    public void Processa(string mensagem)
    {
        using var scope = _scopeFactory.CreateScope();

        var itemRepository = scope.ServiceProvider.GetRequiredService<IItemRepository>();

        var restauranteReadDto = JsonSerializer.Deserialize<RestauranteReadDto>(mensagem);

        var resturante = _mapper.Map<Restaurante>(restauranteReadDto);

        if (!itemRepository.ExisteRestauranteExterno(resturante.Id))
        {
            itemRepository.CreateRestaurante(resturante);
            itemRepository.SaveChanges();
            Console.WriteLine("OK!");
        }
    }
}
