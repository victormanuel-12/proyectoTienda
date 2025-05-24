from substack_api import Newsletter
import json


newsletter = Newsletter("https://astyleset.substack.com")

# Obtener los 5 posts más recientes
recent_posts = newsletter.get_posts(limit=4)

# Lista para almacenar los datos extraídos
posts_data = []

for post in recent_posts:
    metadata = post.get_metadata()  # Este es el método correcto
    posts_data.append({
        "Title": metadata.get("title", ""),
        "Url": metadata.get("canonical_url", ""),  # o "url" si no existe
        "Summary": metadata.get("description", ""),  # o "subtitle", "excerpt"
        "ImageUrl": metadata.get("image") or "" 
    })

# Imprimir como JSON
print(json.dumps(posts_data, ensure_ascii=False))

