"""
Adaptive Card Service - Python equivalent of server/services/AdaptiveCardService.js
Provides exact same functionality as Node.js version using adaptive cards templating
"""

import json
import os
from pathlib import Path
from botbuilder.core import CardFactory

def create_adaptive_card(file_name, task_info, percent_option1=None, percent_option2=None):
    """
    Create adaptive card from template - equivalent to Node.js createAdaptiveCard
    
    Args:
        file_name (str): Name of the template file (e.g., 'Poll.json')
        task_info (dict): Task info data to populate the template
        percent_option1 (int, optional): Percentage for option 1
        percent_option2 (int, optional): Percentage for option 2
    
    Returns:
        dict: Adaptive card attachment object (equivalent to CardFactory.adaptiveCard)
    """
    try:
        # Read JSON file (equivalent to fs.readFileSync)
        project_root = Path(__file__).parent.parent.parent
        template_path = project_root / 'resources' / file_name
        
        print(f"Loading template from: {template_path}")
        
        if not template_path.exists():
            print(f" Template file not found: {template_path}")
            raise FileNotFoundError(f"Template file not found: {file_name}")
        
        # Read file content (equivalent to fs.readFileSync)
        with open(template_path, 'r', encoding='utf-8') as file:
            json_content_str = file.read()
        
        # Parse JSON (equivalent to JSON.parse)
        card_json = json.loads(json_content_str)
        
        # Create template data object (equivalent to ACData.Template.expand)
        root_data = {
            'title': task_info.get('title', ''),
            'option1': task_info.get('option1', ''),
            'option2': task_info.get('option2', ''),
            'Id': task_info.get('Id', ''),
        }
        
        # Add percentages if provided
        if percent_option1 is not None:
            root_data['percentoption1'] = percent_option1
        if percent_option2 is not None:
            root_data['percentoption2'] = percent_option2
        
        print(f" Template data: {root_data}")
        
        # Simple template expansion (replace placeholders)
        card_payload = expand_template(card_json, root_data)
        
        print(f" Expanded card payload: {json.dumps(card_payload, indent=2)}")
        
        # Return adaptive card (equivalent to CardFactory.adaptiveCard)
        return create_card_factory_adaptive_card(card_payload)
        
    except Exception as e:
        print(f"Error creating adaptive card: {str(e)}")
        print(f" Error type: {type(e)}")
        
        # Return error card
        error_card = {
            "type": "AdaptiveCard",
            "version": "1.3",
            "body": [
                {
                    "type": "TextBlock",
                    "text": f"Error creating card: {str(e)}",
                    "color": "Attention"
                }
            ]
        }
        return create_card_factory_adaptive_card(error_card)

def expand_template(template_obj, data):
    """
    Expand template with data - equivalent to ACData.Template.expand
    Simple recursive template expansion for JSON objects
    """
    if isinstance(template_obj, dict):
        result = {}
        for key, value in template_obj.items():
            result[key] = expand_template(value, data)
        return result
    elif isinstance(template_obj, list):
        return [expand_template(item, data) for item in template_obj]
    elif isinstance(template_obj, str):
        # Replace template variables like ${title}, ${option1}, etc.
        result = template_obj
        for key, value in data.items():
            placeholder = f"${{{key}}}"
            if placeholder in result:
                result = result.replace(placeholder, str(value))
        return result
    else:
        return template_obj

def create_card_factory_adaptive_card(card_payload):
    """
    Create adaptive card attachment - equivalent to CardFactory.adaptiveCard
    """
    return {
        "contentType": "application/vnd.microsoft.card.adaptive",
        "content": card_payload
    }
