import { Injectable } from '@angular/core';

export interface AssistantFilter {
  searchTerm?: string;
  inStockOnly?: boolean;
  maxPrice?: number;
  sortBy?: string;
  sortDescending?: boolean;
  featuredOnly?: boolean;
}

export interface AssistantResponse {
  message: string;
  filters: AssistantFilter;
  suggestions: string[];
}

interface Rule {
  pattern: RegExp;
  action: (matches: RegExpMatchArray) => AssistantFilter;
  message: (matches: RegExpMatchArray) => string;
}

@Injectable({ providedIn: 'root' })
export class ShoppingAssistantService {
  private rules: Rule[] = [
    {
      pattern: /(cheap|budget|affordable|low cost|under\s*(\d+))/i,
      action: (m) => {
        const amount = m[2] ? parseInt(m[2]) : undefined;
        return amount ? { maxPrice: amount, sortBy: 'price', sortDescending: false } : { sortBy: 'price', sortDescending: false };
      },
      message: (m) => m[2] ? `Showing budget-friendly products under ${m[2]}` : 'Showing cheapest products first'
    },
    {
      pattern: /(expensive|high end|premium|top\s*(?:of\s*the\s*line|line))/i,
      action: () => ({ sortBy: 'price', sortDescending: true }),
      message: () => 'Showing premium products (highest price first)'
    },
    {
      pattern: /(gaming|gamer|game\s*pc)/i,
      action: () => ({ searchTerm: 'gaming' }),
      message: () => 'Showing gaming products'
    },
    {
      pattern: /(laptop|notebook|ultrabook)/i,
      action: () => ({ searchTerm: 'laptop' }),
      message: () => 'Showing laptops'
    },
    {
      pattern: /(phone|smartphone|mobile)/i,
      action: () => ({ searchTerm: 'phone' }),
      message: () => 'Showing phones'
    },
    {
      pattern: /(in stock|available|ready to ship)/i,
      action: () => ({ inStockOnly: true }),
      message: () => 'Showing only in-stock items'
    },
    {
      pattern: /(featured|best|special|recommended)/i,
      action: () => ({ featuredOnly: true }),
      message: () => 'Showing featured products'
    },
    {
      pattern: /(best rated|top rated|highest rated|highest rated)/i,
      action: () => ({ sortBy: 'rating', sortDescending: true }),
      message: () => 'Showing best rated products'
    },
    {
      pattern: /(wireless|bluetooth|earbuds|headphone)/i,
      action: () => ({ searchTerm: 'wireless' }),
      message: () => 'Showing wireless products'
    },
    {
      pattern: /(tshirt|clothing|shirt)/i,
      action: () => ({ searchTerm: 'tshirt' }),
      message: () => 'Showing t-shirts and clothing'
    },
    {
      pattern: /(smart home|iot|automation)/i,
      action: () => ({ searchTerm: 'smart' }),
      message: () => 'Showing smart home products'
    },
    {
      pattern: /(clear|reset|start over)/i,
      action: () => ({}),
      message: () => 'Filters cleared'
    }
  ];

  parseQuery(query: string): AssistantResponse {
    const cleanQuery = query.toLowerCase().trim();
    
    if (!cleanQuery) {
      return {
        message: 'Try: "cheap laptop", "gaming", "under 5000", "in stock"',
        filters: {},
        suggestions: this.getSuggestions()
      };
    }

    const matchedRules: { rule: Rule; matches: RegExpMatchArray }[] = [];
    
    for (const rule of this.rules) {
      const matches = cleanQuery.match(rule.pattern);
      if (matches) {
        matchedRules.push({ rule, matches });
      }
    }

    if (matchedRules.length === 0) {
      return {
        message: `Searching for "${query}"`,
        filters: { searchTerm: query },
        suggestions: this.getSuggestions()
      };
    }

    const filters: AssistantFilter = {};
    const messages: string[] = [];
    
    for (const { rule, matches } of matchedRules) {
      const action = rule.action(matches);
      Object.assign(filters, action);
      messages.push(rule.message(matches));
    }

    const message = messages.length === 1 
      ? messages[0] 
      : `Applied filters: ${messages.join(' + ')}`;

    return { message, filters, suggestions: this.getSuggestions() };
  }

  getSuggestions(): string[] {
    return [
      'Cheap products',
      'Gaming gear',
      'Under 5000',
      'In stock',
      'Best rated',
      'Featured'
    ];
  }

  resetFilters(): AssistantFilter {
    return {};
  }
}
