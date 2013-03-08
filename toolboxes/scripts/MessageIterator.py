import xml.dom.minidom
import os.path
import DictionaryConstants

class MessageIterator:
    def __init__(self, messageFileName):
        self.messageFileName = messageFileName

        if not os.path.isfile(messageFileName) :
            msg = "File not found: " + messageFileName
            raise IOError(msg)

        doc = xml.dom.minidom.parse(messageFileName)
        self.messageNodes = doc.getElementsByTagName("message")
        self.currentMessageIndex = 0
        self.lastMessageIndex = len(self.messageNodes) - 1

    def getMessageSicControlPointsFromXml(self, messageElementXml) : 

        node = messageElementXml

        if node.nodeType != xml.dom.Node.ELEMENT_NODE :
            return None, None, None
        
        attributes = { }

        # print 'Element name: %s' % node.nodeName

        sic = node.getElementsByTagName(DictionaryConstants.Tag_SymbolId)[0].childNodes[0].data
        controlPoints = node.getElementsByTagName(DictionaryConstants.Tag_ControlPoints)[0].childNodes[0].data

        childNodes = node.getElementsByTagName("*")

        for childNode in childNodes:
            if childNode.nodeType == childNode.ELEMENT_NODE:
                # print childNode.tagName, childNode.childNodes[0].data
                tag = childNode.tagName
                if (not(tag == DictionaryConstants.Tag_SymbolId) \
                    or (tag == DictionaryConstants.Tag_ControlPoints)) :
                    attributes[tag] = childNode.childNodes[0].data

        return sic, controlPoints, attributes

    def __iter__(self) :
        return self

    def next(self) :
        if self.currentMessageIndex > self.lastMessageIndex:
            raise StopIteration
        else:
            node = self.messageNodes[self.currentMessageIndex]
            sic, controlPoints, attributes = self.getMessageSicControlPointsFromXml(node)
            self.currentMessageIndex += 1

            return sic, controlPoints, attributes