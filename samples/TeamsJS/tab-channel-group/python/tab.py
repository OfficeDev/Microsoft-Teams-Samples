"""
Tab class for Microsoft Teams tab functionality
"""

class Tab:
    def __init__(self):
        self.colors = {
            'gray': 'You have selected the Gray tab! ',
            'red': 'You have selected the Red tab! '
        }
    
    def get_color(self, color):
        """Get color message based on selection"""
        return self.colors.get(color.lower(), f'Color {color} not found')
